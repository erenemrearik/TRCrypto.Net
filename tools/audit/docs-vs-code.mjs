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

import { collectFacts } from '../lib/facts.mjs';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const read = (p) => readFileSync(resolve(root, p), 'utf8');

const facts = collectFacts(root);

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
  { name: 'CoinTR', src: 'src/TRCrypto.CoinTR', readme: 'src/TRCrypto.CoinTR/README.md' },
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

// Sayim `tools/lib/facts.mjs` icinde; site de ayni modulu kullanir. Iki yerde ayri
// sayilsaydi iki taraf farkli sonuc uretebilir ve hangisinin dogru oldugu belirsiz
// kalirdi. Sayim ayrica test projelerini diskten bulur, bu yuzden yeni bir proje
// eklendiginde burada isim guncellemek gerekmez.
const unitTotal = facts.unitTests;

// Test sayisi yalnizca durum belgesinde yazili. README'de rozet olarak tutulmuyordu:
// elle guncellenen bir sayi her test eklendiginde bayatliyor ve derleme rozeti zaten
// yesil mi kirmizi mi oldugunu soyluyor.
{
  const claimed = read('docs/DURUM.md').match(/(\d+)\/\d+ birim/)?.[1];

  if (!claimed) {
    note('Testler', 'docs/DURUM.md icinde birim test sayisi bulunamadi');
  } else if (Number(claimed) !== unitTotal) {
    note('Testler', `docs/DURUM.md ${claimed} birim testi bildiriyor, kodda ${unitTotal} var`);
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

/**
 * Siteye bilincli olarak alinmayan markdown dosyalari ve nedenleri.
 *
 * Liste gerekce ister. Once yalnizca iki klasor taraniyordu, dolayisiyla ornek
 * uygulamanin ve marka varliklarinin belgeleri kimsenin fark etmedigi bir bosluga
 * dusuyordu: depoda vardilar, sitede yoklardi. Artik butun .md dosyalari taranir ve
 * disarida kalan her dosya burada adiyla yazilmak zorundadir.
 */
const siteExclusions = {
  'README.md': 'Icerigi sitenin giris sayfasinda yeniden yazildi',
  'CLAUDE.md': 'Depoda calisan ajanlar icin talimat; kullanici belgesi degil',
  '.github/pull_request_template.md': 'GitHub formu; okunacak bir belge degil',
};

/** Depodaki tum markdown dosyalari. */
function markdownFiles() {
  const out = [];
  // `.claude` altindaki dosyalar CLAUDE.md ile ayni kategoridedir: depoda calisan
  // ajanlarin talimatlari, kullanicinin okuyacagi belgeler degil. Klasor olarak
  // atlanir ki yeni bir skill eklendiginde burasi guncellenmek zorunda kalmasin.
  const skip = new Set(['node_modules', 'bin', 'obj', '.git', '.claude', 'artifacts']);

  const walk = (current) => {
    for (const entry of readdirSync(resolve(root, current || '.'), { withFileTypes: true })) {
      if (skip.has(entry.name)) continue;
      const next = current ? join(current, entry.name) : entry.name;
      if (entry.isDirectory()) walk(next);
      else if (entry.name.endsWith('.md')) out.push(next.replace(/\\/g, '/'));
    }
  };

  walk('');
  return out;
}

for (const file of markdownFiles()) {
  if (navFiles.includes(file)) continue;
  if (file in siteExclusions) continue;

  note('Site', `${file} dokumani sitede gorunmuyor ve disarida birakma gerekcesi yazilmamis`);
}

for (const file of Object.keys(siteExclusions)) {
  if (!existsSync(resolve(root, file))) {
    note('Site', `disarida birakilan ${file} dosyasi artik yok; listeden cikarilmali`);
  }
}

// ── 7. Hedef platform listesi tutarli mi? ──

const targetFrameworks = read('src/Directory.Build.props').match(/<TargetFrameworks>([^<]+)</)?.[1]
  ?? read('Directory.Build.props').match(/<TargetFrameworks>([^<]+)</)?.[1];

if (targetFrameworks) {
  const frameworks = targetFrameworks.split(';').map((s) => s.trim()).filter(Boolean);
  for (const file of [
    'README.md',
    'CLAUDE.md',
    'src/TRCrypto.BtcTurk/README.md',
    'src/TRCrypto.BinanceTR/README.md',
    'src/TRCrypto.CoinTR/README.md',
  ]) {
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

// ── 8. Cok dilli README ayrismis mi? ──

// Ceviriler elle tutuluyor ve kaynak degisince sessizce geride kaliyor. Cevirinin
// dogrulugu makineyle olculemez, ama YAPISAL ayrisma olculur: bolum sayisi, rozet sayisi
// ve kod bloklarinin sayisi iki dosyada da ayni olmalidir. Birine bolum eklendiginde ya da
// rozet degistiginde bu sayilar tutmaz ve okuyucu eksik bir ceviriyle bas basa kalir.
{
  const pairs = [['README.md', 'README.en.md']];

  const sections = (t) => (t.match(/^#{1,3} /gm) ?? []).length;
  const badges = (t) => (t.match(/\[!\[[^\]]*\]\(https:\/\/img\.shields\.io/g) ?? []).length;
  const fences = (t) => (t.match(/^```\w*/gm) ?? []).length;

  for (const [source, translation] of pairs) {
    const original = read(source);
    const translated = read(translation);

    const checks = [
      ['bolum', sections],
      ['rozet', badges],
      ['kod blogu', fences],
    ];

    for (const [label, count] of checks) {
      if (count(original) !== count(translated)) {
        note(
          'Ceviri',
          `${label} sayisi ayristi: ${source} ${count(original)}, ${translation} ${count(translated)}`
        );
      }
    }

    // Dil secici her iki dosyada da bulunmali; yoksa okuyucu digerine gecemez.
    if (!original.includes(translation)) note('Ceviri', `${source} dil secici tasimiyor`);
    if (!translated.includes(source)) note('Ceviri', `${translation} dil secici tasimiyor`);
  }
}

// ── 9. Fixture'lar secret taramasini tetikleyecek mi? ──

// Bir fixture'a dokumantasyondan kopyalanan yuksek entropili bir deger, gercek olmasa
// bile secret tarayicisini kirmiziya dondurur ve derlemeyi durdurur. Bunu tarayiciyi
// bekleyerek ogrenmek pahalidir; burada yazarken yakalanir.
//
// Olcut gitleaks'in generic-api-key kuralinin sezgisine yakindir: uzun ve yuksek
// entropili dize. Imzalama test vektorleri bilincli olarak muaf tutulmustur; onlar
// beklenen HMAC ciktilaridir ve `.gitleaks.toml` icinde de ayni gerekceyle muaftir.
{
  const shannon = (value) => {
    const counts = new Map();
    for (const char of value) counts.set(char, (counts.get(char) ?? 0) + 1);

    let bits = 0;
    for (const count of counts.values()) {
      const p = count / value.length;
      bits -= p * Math.log2(p);
    }
    return bits;
  };

  const exempt = [
    'tests/TRCrypto.BtcTurk.UnitTests/AuthenticationTests.cs',
    'tests/TRCrypto.BinanceTR.UnitTests/AuthenticationTests.cs',
    'tests/TRCrypto.CoinTR.UnitTests/AuthenticationTests.cs',
    'tests/TRCrypto.BtcTurk.UnitTests/CredentialsSecurityTests.cs',
  ];

  const fixtureDirs = [
    'tests/TRCrypto.BtcTurk.UnitTests/Fixtures',
    'tests/TRCrypto.BinanceTR.UnitTests/Fixtures',
    'tests/TRCrypto.CoinTR.UnitTests/Fixtures',
  ];

  for (const dir of fixtureDirs) {
    if (!existsSync(resolve(root, dir))) continue;

    for (const name of readdirSync(resolve(root, dir))) {
      const file = join(dir, name).replace(/\\/g, '/');
      if (exempt.includes(file)) continue;

      for (const match of read(file).matchAll(/[A-Za-z0-9+/=_-]{32,}/g)) {
        const value = match[0];
        if (/^\d+$/.test(value)) continue;
        if (shannon(value) <= 4.4) continue;

        note(
          'Fixture',
          `${file} icinde ${value.length} karakterlik yuksek entropili deger var; ` +
          'secret tarayicisi bunu kimlik bilgisi sanir. Acikca sahte bir deger kullanin.'
        );
      }
    }
  }
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
