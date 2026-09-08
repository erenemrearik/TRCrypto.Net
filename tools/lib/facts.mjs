// Depoyu okuyup projenin sayilabilir gerceklerini cikarir.
//
// Bu dosya, giris sayfasindaki sayilarin elle yazilmis olmasindan dogdu. Test sayisi,
// sapma sayisi ve paket durumu sablonda sabit metindi; kod ilerledikce sessizce
// eskidiler ve site "2 borsa, 212 test, NuGet'e yayinlanmadi" demeye devam etti.
//
// Artik hicbiri elle yazilmaz. Site de denetim betigi de buradan okur, dolayisiyla
// ikisi ayrisamaz: ayrisma ancak ayni sayiyi iki yerde tutunca mumkundur.
//
// Kullanim:  import { collectFacts } from '../lib/facts.mjs'

import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join, resolve } from 'node:path';

/** Bir dizin agacindaki tum .cs dosyalarini toplar; uretilen ciktiyi atlar. */
function sourceFiles(root, dir) {
  const out = [];
  const walk = (current) => {
    if (!existsSync(resolve(root, current))) return;
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

/**
 * Bir test projesindeki calistirma sayisini sayar.
 *
 * Theory'lerde her InlineData ayri bir calistirmadir; xUnit'in bildirdigi sayi budur.
 * Yalnizca metot sayisini saymak, raporlanan sayidan dusuk cikar ve belgeye yazildiginda
 * yanlis olur.
 */
function countTests(root, projectDir) {
  let total = 0;

  for (const file of sourceFiles(root, projectDir)) {
    const text = readFileSync(resolve(root, file), 'utf8');
    total += (text.match(/\[Fact\]|\[SkippableFact\]/g) ?? []).length;

    for (const block of text.split('[Theory]').slice(1)) {
      const head = block.split(/public\s/)[0];
      total += (head.match(/\[InlineData/g) ?? []).length;
    }
  }

  return total;
}

/** Verilen sonekle biten test projelerinin toplam calistirma sayisi. */
function countTestsBySuffix(root, suffix) {
  const testsDir = resolve(root, 'tests');
  if (!existsSync(testsDir)) return 0;

  return readdirSync(testsDir, { withFileTypes: true })
    .filter((e) => e.isDirectory() && e.name.endsWith(suffix))
    .reduce((sum, e) => sum + countTests(root, join('tests', e.name)), 0);
}

/**
 * Yayinlanabilir paketleri diskten cikarir.
 *
 * Liste elle tutulsaydi yeni bir adaptor eklendiginde siteye yazilmasi unutulabilirdi;
 * proje bunu bir kez yasadi. Kaynak dizin tek dogru listedir.
 */
function collectPackages(root) {
  const srcDir = resolve(root, 'src');
  if (!existsSync(srcDir)) return [];

  const packages = [];

  for (const entry of readdirSync(srcDir, { withFileTypes: true })) {
    if (!entry.isDirectory()) continue;

    const project = join('src', entry.name, `${entry.name}.csproj`);
    if (!existsSync(resolve(root, project))) continue;

    const xml = readFileSync(resolve(root, project), 'utf8');
    const packageId = xml.match(/<PackageId>([^<]+)</)?.[1] ?? entry.name;

    // Sayfa kimligi burada uretilmez. Uretilseydi "TRCrypto.BtcTurk" adindan
    // "btc-turk" cikardi ve sitedeki mevcut #/btcturk adresi kirilirdi. Site,
    // paketi README yoluna bakarak kendi gezinti kimligiyle eslestirir.
    packages.push({
      id: packageId,
      readme: join('src', entry.name, 'README.md').replace(/\\/g, '/'),
      description: xml.match(/<Description>([^<]+)</)?.[1] ?? '',
    });
  }

  return packages.sort((a, b) => a.id.localeCompare(b.id, 'en'));
}

/**
 * Yayinlanmis surumu degisiklik gunlugunden okur.
 *
 * Etiketten okumak daha dogrudan olurdu ama site derlemesi git gecmisine erisemedigi
 * yerlerde de calismali. Gunlukteki ilk surum basligi depoda tutulan tek yazili
 * kaynaktir.
 */
function readVersion(root) {
  const changelog = readFileSync(resolve(root, 'CHANGELOG.md'), 'utf8');
  const match = changelog.match(/^##\s*\[([^\]]+)\](?:\s*-\s*(.+))?$/m);

  if (!match || /yay[ıi]nlanmad|unreleased/i.test(match[1])) {
    return { version: null, date: null, published: false };
  }

  return {
    version: match[1].trim(),
    date: match[2]?.trim() ?? null,
    published: true,
    prerelease: match[1].includes('-'),
  };
}

/** Projenin sayilabilir gercekleri. */
export function collectFacts(root) {
  const buildProps = existsSync(resolve(root, 'src/Directory.Build.props'))
    ? readFileSync(resolve(root, 'src/Directory.Build.props'), 'utf8')
    : readFileSync(resolve(root, 'Directory.Build.props'), 'utf8');

  const frameworks = (buildProps.match(/<TargetFrameworks>([^<]+)</)?.[1] ?? '')
    .split(';')
    .map((s) => s.trim())
    .filter(Boolean);

  const spec = readFileSync(
    resolve(root, 'docs/spec/TRCrypto-teknik-spesifikasyon-v1.0.md'),
    'utf8'
  );

  // Sapmalar D-1 ile baslayip artarak numaralanir; en buyuk numara toplam sayidir.
  const deviations = Math.max(
    0,
    ...[...spec.matchAll(/^#{2,4}\s*D-(\d+)/gm)].map((m) => Number(m[1]))
  );

  return {
    packages: collectPackages(root),
    unitTests: countTestsBySuffix(root, '.UnitTests'),
    liveTests: countTestsBySuffix(root, '.IntegrationTests'),
    deviations,
    frameworks,
    release: readVersion(root),
  };
}
