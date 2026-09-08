// Uretilen sitenin markdown isleyicisini DOM olmadan calistirir.
//
// Isleyici site icinde yasadigi icin normal birim testleriyle kapsanmiyor; bu betik
// onu tarayici disinda her dokumana karsi calistirir ve sessizce bozulan yapilari
// yakalar: kapanmamis etiket, isleme sirasinda atilan istisna, bos cikan sayfa,
// cevrilmemis depo baglantisi.
//
// Kullanim:  node tools/site/check.mjs

import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import vm from 'node:vm';

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, '..', '..');

const html = readFileSync(resolve(root, 'docs', 'index.html'), 'utf8');

const scriptStart = html.indexOf('<script>');
const script = html.slice(scriptStart + 8, html.lastIndexOf('</' + 'script>'));

// Isleyici, DOM'a dokunan uygulama kodundan once biter.
const appMarker = script.indexOf('/* ═══ Uygulama');
const pure = script.slice(0, appMarker);

const context = { console, module: {} };
vm.createContext(context);
vm.runInContext(
  pure + '\nthis.__api = { render, inline, slug, DOCS, NAV, FACTS, homePage, highlight };',
  context
);

const { render, DOCS, NAV, FACTS, homePage } = context.__api;

const problems = [];
let totalHeadings = 0;
let totalHtml = 0;

for (const group of NAV) {
  for (const page of group.pages) {
    const doc = DOCS[page.id];
    if (!doc) {
      problems.push(`${page.id}: gezinti listesinde var ama icerik yok`);
      continue;
    }
    if (doc.body === null) continue; // giris sayfasi sablonda elle yazili

    let out;
    try {
      out = render(doc.body);
    } catch (error) {
      problems.push(`${page.id}: isleme sirasinda hata: ${error.message}`);
      continue;
    }

    totalHeadings += out.headings.length;
    totalHtml += out.html.length;

    if (out.html.trim().length < 40) problems.push(`${page.id}: cikti bos`);

    // Etiket dengesi: acilan her blok etiketi kapanmali.
    for (const tag of ['div', 'table', 'pre', 'code', 'ul', 'ol', 'li', 'blockquote', 'p', 'tr', 'td', 'th']) {
      const open = (out.html.match(new RegExp('<' + tag + '(?=[\\s>])', 'g')) ?? []).length;
      const close = (out.html.match(new RegExp('</' + tag + '>', 'g')) ?? []).length;
      if (open !== close) problems.push(`${page.id}: <${tag}> dengesiz: ${open} acilis, ${close} kapanis`);
    }

    // Isaretlemenin metne sizmasi: govdede kalan markdown izleri.
    const leaked = out.html.match(/^(#{2,6} |\| )/m);
    if (leaked) problems.push(`${page.id}: islenmemis markdown kalmis: ${JSON.stringify(leaked[0])}`);

    // Baglantilar: depo icindeki .md yollari hash rotasina cevrilmis olmali.
    const mdLink = out.html.match(/href="(?!https?:)[^"]*\.md[^"]*"/);
    if (mdLink) problems.push(`${page.id}: cevrilmemis dokuman baglantisi: ${mdLink[0]}`);
  }
}

/* ── Giris sayfasi ────────────────────────────────────────────────────────────
   Giris sayfasi markdown'dan degil sablondan gelir, dolayisiyla yukaridaki
   dongunun disindadir. Bir sure kimse ona bakmadi ve icerigi sessizce eskidi:
   depoda uc borsa varken sayfa iki borsa, 277 test varken 212 test ve
   yayinlanmis bir surum varken "NuGet'e yayinlanmadi" diyordu.

   Sayilar artik FACTS'ten geliyor. Asagidaki kontroller bunu dogrular, yani
   birinin sayilari tekrar sabit metne cevirmesi durumunda derleme durur.
   ───────────────────────────────────────────────────────────────────────── */
{
  let home;
  try {
    home = homePage();
  } catch (error) {
    problems.push(`giris: sayfa uretilemedi: ${error.message}`);
    home = '';
  }

  if (home) {
    for (const tag of ['div', 'table', 'pre', 'code', 'tr', 'td', 'th', 'dl', 'dt', 'dd']) {
      const open = (home.match(new RegExp('<' + tag + '(?=[\\s>])', 'g')) ?? []).length;
      const close = (home.match(new RegExp('</' + tag + '>', 'g')) ?? []).length;
      if (open !== close) problems.push(`giris: <${tag}> dengesiz: ${open} acilis, ${close} kapanis`);
    }

    // Her paket giris sayfasinda gorunmeli. Yeni bir adaptor eklendiginde bunu
    // hatirlamak gerekmesin diye kontrol listeden degil diskten turetilir.
    for (const pkg of FACTS.packages) {
      if (!home.includes(pkg.id)) problems.push(`giris: ${pkg.id} paketi sayfada yok`);
    }

    // Sayilar sayfada gercekten yaziyor mu?
    for (const [ad, deger] of [
      ['birim test', FACTS.unitTests],
      ['sapma', FACTS.deviations],
    ]) {
      if (!home.includes(String(deger))) {
        problems.push(`giris: ${ad} sayisi (${deger}) sayfada gorunmuyor`);
      }
    }

    // Yayin durumu ile sayfanin dili ayrismamali.
    if (FACTS.release.published) {
      if (/yay[ıi]nlanmad/i.test(home)) {
        problems.push('giris: surum yayinda ama sayfa hala yayinlanmadigini soyluyor');
      }
      if (!home.includes(FACTS.release.version)) {
        problems.push(`giris: yayinlanan surum (${FACTS.release.version}) sayfada gorunmuyor`);
      }
    }
  }
}

const pages = Object.keys(DOCS).length;

if (problems.length) {
  console.error(`${problems.length} sorun bulundu:\n`);
  for (const problem of problems) console.error('  - ' + problem);
  process.exit(1);
}

console.log(
  `${pages} sayfa islendi, ${totalHeadings} baslik cikarildi, ` +
  `${(totalHtml / 1024).toFixed(0)} KB HTML uretildi, sorun yok`
);
