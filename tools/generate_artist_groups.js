#!/usr/bin/env node
/*
 * Generates the Swift and C# `WikiArtArtistGroup` literals (grouped by
 * WikiArt's own A-Z alphabet index) from tools/wikiart_artists.json.
 *
 * Usage: node tools/generate_artist_groups.js
 * Writes: tools/generated_artist_groups.swift.txt
 *         tools/generated_artist_groups.cs.txt
 */

const fs = require('fs');
const path = require('path');

const artists = JSON.parse(fs.readFileSync(path.join(__dirname, 'wikiart_artists.json'), 'utf8'));

const byLetter = new Map();
for (const a of artists) {
  if (!byLetter.has(a.letter)) byLetter.set(a.letter, []);
  byLetter.get(a.letter).push(a);
}

// Sort each letter's list by last name (last whitespace-separated token) so
// artists are easy to find within a letter, not just by first name.
function lastNameKey(name) {
  const parts = name.trim().split(/\s+/);
  return parts[parts.length - 1];
}

const letters = [...byLetter.keys()].sort((a, b) => a.localeCompare(b, 'en'));
for (const letter of letters) {
  byLetter.get(letter).sort((a, b) => {
    const cmp = lastNameKey(a.name).localeCompare(lastNameKey(b.name), 'en');
    return cmp !== 0 ? cmp : a.name.localeCompare(b.name, 'en');
  });
}

function swiftEscape(s) {
  return s.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}

function csEscape(s) {
  return s.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}

let swift = '';
for (const letter of letters) {
  swift += `    WikiArtArtistGroup(name: "${letter}", artists: [\n`;
  for (const a of byLetter.get(letter)) {
    swift += `        WikiArtArtist(name: "${swiftEscape(a.name)}", slug: "${a.slug}"),\n`;
  }
  swift += '    ]),\n';
}
fs.writeFileSync(path.join(__dirname, 'generated_artist_groups.swift.txt'), swift);

let cs = '';
for (const letter of letters) {
  cs += `        new WikiArtArtistGroup("${letter}", new WikiArtArtist[]\n        {\n`;
  for (const a of byLetter.get(letter)) {
    cs += `            new("${csEscape(a.name)}", "${a.slug}"),\n`;
  }
  cs += '        }),\n';
}
fs.writeFileSync(path.join(__dirname, 'generated_artist_groups.cs.txt'), cs);

console.log(`Letters: ${letters.length}, total artists: ${artists.length}`);
console.log('Wrote generated_artist_groups.swift.txt and generated_artist_groups.cs.txt');
