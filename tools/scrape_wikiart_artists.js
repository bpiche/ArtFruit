#!/usr/bin/env node
/*
 * Scrapes the full WikiArt artist list from the alphabet index pages.
 *
 *   https://www.wikiart.org/en/alphabet/a/text-list
 *   ... through /z, plus the numeric bucket.
 *
 * The "text-list" layout renders every artist for a letter as a plain
 * <a href="/en/<slug>">Display Name</a> inside <ul class="alphabet-container-text">,
 * which is far cheaper (and more reliable) than paging the JSON API.
 *
 * Output: tools/wikiart_artists.json  ->  [{ name, slug, letter }, ...]
 *
 * Usage: node tools/scrape_wikiart_artists.js
 */

const fs = require('fs');
const path = require('path');

const LETTERS = '0abcdefghijklmnopqrstuvwxyz'.split('');
const UA =
  'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36';

// Slugs that are site navigation rather than artists.
const NON_ARTIST = new Set([
  'artists-by-art-movement', 'artists-by-painting-school', 'artists-by-genre',
  'artists-by-field', 'artists-by-nation', 'artists-by-century',
  'artists-by-art-institution', 'paintings-by-style', 'paintings-by-genre',
  'paintings-by-media', 'artistadvancedsearch', 'paintingadvancedsearch',
  'chronological-artists', 'popular-artists', 'female-artists',
  'recently-added-artists', 'recently-added-artworks', 'high-resolution-artworks',
  'about', 'terms-of-use', 'privacy-policy', 'donate', 'artist-of-the-day',
  'alphabet', 'artists', 'paintings', 'search', 'app', 'store', 'shop',
  'short-of-the-month', 'movies', 'dictionaries', 'popular-paintings',
]);

const ENTITIES = {
  amp: '&', lt: '<', gt: '>', quot: '"', apos: "'", nbsp: ' ', '#39': "'",
};

function decodeEntities(s) {
  return s
    .replace(/&#(\d+);/g, (_, d) => String.fromCharCode(parseInt(d, 10)))
    .replace(/&#x([0-9a-f]+);/gi, (_, h) => String.fromCharCode(parseInt(h, 16)))
    .replace(/&([a-z]+);/gi, (m, e) => (ENTITIES[e.toLowerCase()] !== undefined ? ENTITIES[e.toLowerCase()] : m));
}

async function fetchLetter(letter) {
  const url = `https://www.wikiart.org/en/alphabet/${letter}/text-list`;
  const res = await fetch(url, { headers: { 'User-Agent': UA, 'Accept-Language': 'en-US,en;q=0.9' } });
  if (!res.ok) throw new Error(`${url} -> HTTP ${res.status}`);
  return res.text();
}

function parseArtists(html, letter) {
  // Narrow to the artist list block so we skip header/footer navigation.
  const start = html.indexOf('class="masonry-text-view masonry-text-view-all"');
  const scoped = start === -1 ? html : html.slice(start);
  const end = scoped.indexOf('</ul>');
  const block = end === -1 ? scoped : scoped.slice(0, end);

  const out = [];
  const seen = new Set();
  const re = /<a[^>]+href="\/en\/([^"\/?#]+)"[^>]*>([^<]+)<\/a>/gi;
  let m;
  while ((m = re.exec(block)) !== null) {
    const slug = m[1].trim();
    const name = decodeEntities(m[2]).replace(/\s+/g, ' ').trim();
    if (!slug || !name) continue;
    if (NON_ARTIST.has(slug)) continue;
    if (slug.includes('/')) continue;
    if (seen.has(slug)) continue;
    seen.add(slug);
    out.push({ name, slug, letter: letter === '0' ? '#' : letter.toUpperCase() });
  }
  return out;
}

(async () => {
  const all = [];
  const bySlug = new Map();

  for (const letter of LETTERS) {
    process.stdout.write(`  ${letter === '0' ? '#' : letter.toUpperCase()} ... `);
    let artists = [];
    for (let attempt = 1; attempt <= 3; attempt++) {
      try {
        artists = parseArtists(await fetchLetter(letter), letter);
        break;
      } catch (err) {
        if (attempt === 3) {
          console.log(`FAILED (${err.message})`);
        } else {
          await new Promise((r) => setTimeout(r, 1500 * attempt));
        }
      }
    }
    let added = 0;
    for (const a of artists) {
      if (bySlug.has(a.slug)) continue;
      bySlug.set(a.slug, a);
      all.push(a);
      added++;
    }
    console.log(`${added} artists`);
    await new Promise((r) => setTimeout(r, 400)); // be polite
  }

  all.sort((a, b) => a.name.localeCompare(b.name, 'en'));

  const outPath = path.join(__dirname, 'wikiart_artists.json');
  fs.writeFileSync(outPath, JSON.stringify(all, null, 2));
  console.log(`\nTotal: ${all.length} artists -> ${outPath}`);
})();
