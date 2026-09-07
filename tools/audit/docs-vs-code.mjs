// Dokumanlarin iddia ettikleriyle kodun gercegini karsilastirir.
//
// Bu depoda dokumanlar kodun yaninda elle guncelleniyor ve ikisi zamanla ayrisiyor.
// Ayrisma sessizdir: bir uc eklenip README'ye yazilmazsa kimse fark etmez, silinen bir
// yontem dokumanda kalmaya devam eder ve kutuphaneyi ilk kez kullanan kisi olmayan bir
// yontemi cagirir.
//
// Betik iddia uretmez; yalnizca iki tarafi karsilastirir ve farki bildirir.
//
// Kullanim:  node tools/audit/docs-vs-code.mjs

import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const read = (p) => readFileSync(resolve(root, p), 'utf8');

const findings = [];
const note = (area, message) => findings.push({ area, message });

/** Bir dizin agacindaki tum .cs dosyalarini toplar; uretilen ciktiyi atlar. */
function sourceFiles(dir) {
  const out = [];
  const walk = (current) => {
    for (const entry of readdirSync(resolve(root, current), { withFileTypes: true })) {
      if (entry.name === 'obj' || entry.name === 'bin') continue;
      const next = join(current, entry.name);
      if (entry.isDirectory()) walk(next);
      else if (entry.name.endsWith('.cs')) out.push(next);
    }
  };
  walk(dir);
  return out;
}

const adapters = [
  { name: 'BtcTurk', src: 'src/TRCrypto.BtcTurk', readme: 'src/TRCrypto.BtcTurk/README.md' },
  { name: 'BinanceTR', src: 'src/TRCrypto.BinanceTR', readme: 'src/TRCrypto.BinanceTR/README.md' },
];

// ── 1. Kodda gecen uc yollari paket README'sinde ve vendor envanterinde var mi? ──

const vendorText = readdirSync(resolve(root, 'docs/vendor'))
  .filter((f) => f.endsWith('.md'))
  .map((f) => read(join('docs/vendor', f)))
  .join('\n');

for (const adapter of adapters) {
  const code = sourceFiles(adapter.src).map((f) => read(f)).join('\n');
  const readme = read(adapter.readme);

  const paths = [...new Set(
    [...code.matchAll(/"(\/(?:api|open)\/v\d[^"]*)"/g)].map((m) => m[1])
  )].sort();

  for (const path of paths) {
    // Yol sablonlu olabilir; karsilastirma icin degisken kismi atilir.
    const stem = path.split('{')[0].replace(/\/$/, '');
    if (!vendorText.includes(stem)) {
      note(adapter.name, `kodda kullanilan ${path} vendor envanterinde gecmiyor`);
    }
    if (!readme.includes(stem)) {
      note(adapter.name, `kodda kullanilan ${path} paket README'sinde gecmiyor`);
    }
  }

  // ── 2. Public arayuz yontemleri README'de anilmis mi? ──

  const interfaceDir = join(adapter.src, 'Interfaces');
  const publicMethods = [...new Set(
    sourceFiles(interfaceDir)
      .map((f) => read(f))
      .join('\n')
      .match(/\b(?:Get|Place|Cancel|Subscribe)\w*Async\b/g) ?? []
  )].sort();

  for (const method of publicMethods) {
    if (!readme.includes(method)) {
      note(adapter.name, `${method} arayuzde var, paket README'sinde anilmiyor`);
    }
  }

  // ── 3. README'de anilan yontemler gercekten var mi? ──

  const claimed = [...new Set(readme.match(/\b(?:Get|Place|Cancel|Subscribe)\w*Async\b/g) ?? [])];
  for (const method of claimed) {
    if (!code.includes(method)) {
      note(adapter.name, `README ${method} yontemini anlatiyor ama kodda yok`);
    }
  }

  // ── 4. Bildirilen shared arayuzler gercekten uygulanmis mi? ──

  const sharedFiles = sourceFiles(adapter.src).filter((f) => f.includes('Shared'));
  const sharedCode = sharedFiles.map((f) => read(f)).join('\n');
  const declared = [...new Set(readme.match(/\bI(?:Spot|Futures|Balance|OrderBook|RecentTrade|Kline|Ticker|Trade|BookTicker)\w*(?:Rest|Socket)Client\b/g) ?? [])];

  for (const iface of declared) {
    if (!sharedCode.includes(iface)) {
      note(adapter.name, `README ${iface} arayuzunu bildiriyor ama shared yuzeyde uygulanmamis`);
    }
  }
}

// ── 5. Test sayilari dokumanlarda dogru mu? ──

const testCount = (project) => {
  const files = sourceFiles(`tests/${project}`);
  let facts = 0;
  for (const file of files) {
    const text = read(file);
    facts += (text.match(/\[Fact\]|\[SkippableFact\]/g) ?? []).length;
    // Theory'lerde her InlineData ayri bir test calistirmasidir.
    for (const block of text.split('[Theory]').slice(1)) {
      const head = block.split(/public\s/)[0];
      facts += (head.match(/\[InlineData/g) ?? []).length;
    }
  }
  return facts;
};

const unitTotal = testCount('TRCrypto.BtcTurk.UnitTests') + testCount('TRCrypto.BinanceTR.UnitTests');

const claimedCounts = [
  ['README.md', read('README.md').match(/testler-(\d+)%20/)?.[1]],
  ['docs/DURUM.md', read('docs/DURUM.md').match(/(\d+)\/\d+ birim/)?.[1]],
];

for (const [file, claimed] of claimedCounts) {
  if (!claimed) {
    note('Testler', `${file} icinde birim test sayisi bulunamadi`);
  } else if (Number(claimed) !== unitTotal) {
    note('Testler', `${file} ${claimed} birim testi bildiriyor, kodda ${unitTotal} var`);
  }
}

// ── 6. Site gezintisindeki her dosya var mi, her dokuman gezintide mi? ──

const buildScript = read('tools/site/build.mjs');
const navFiles = [...buildScript.matchAll(/file: '([^']+)'/g)].map((m) => m[1]);

for (const file of navFiles) {
  if (!existsSync(resolve(root, file))) {
    note('Site', `gezinti listesindeki ${file} dosyasi yok`);
  }
}

const docFiles = readdirSync(resolve(root, 'docs/vendor'))
  .filter((f) => f.endsWith('.md'))
  .map((f) => 'docs/vendor/' + f)
  .concat(
    readdirSync(resolve(root, 'docs/credentials'))
      .filter((f) => f.endsWith('.md'))
      .map((f) => 'docs/credentials/' + f)
  );

for (const file of docFiles) {
  if (!navFiles.includes(file)) {
    note('Site', `${file} dokumani sitede gorunmuyor`);
  }
}

// ── 7. Hedef platform listesi tutarli mi? ──

const targetFrameworks = read('src/Directory.Build.props').match(/<TargetFrameworks>([^<]+)</)?.[1]
  ?? read('Directory.Build.props').match(/<TargetFrameworks>([^<]+)</)?.[1];

if (targetFrameworks) {
  const frameworks = targetFrameworks.split(';').map((s) => s.trim()).filter(Boolean);
  for (const file of ['README.md', 'CLAUDE.md', 'src/TRCrypto.BtcTurk/README.md', 'src/TRCrypto.BinanceTR/README.md']) {
    const text = read(file);
    for (const framework of frameworks) {
      if (!text.includes(framework)) {
        note('Platform', `${file} hedef platform ${framework} degerini anmiyor`);
      }
    }
  }
} else {
  note('Platform', 'TargetFrameworks tanimi bulunamadi');
}

// ── Rapor ──

if (!findings.length) {
  console.log('Dokumanlar kodla tutarli.');
  process.exit(0);
}

const grouped = new Map();
for (const { area, message } of findings) {
  if (!grouped.has(area)) grouped.set(area, []);
  grouped.get(area).push(message);
}

console.log(`${findings.length} tutarsizlik bulundu.\n`);
for (const [area, messages] of grouped) {
  console.log(`${area}:`);
  for (const message of messages) console.log('  - ' + message);
  console.log();
}
process.exit(1);
