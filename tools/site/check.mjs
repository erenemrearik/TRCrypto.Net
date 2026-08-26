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
vm.runInContext(pure + '\nthis.__api = { render, inline, slug, DOCS, NAV, highlight };', context);

const { render, DOCS, NAV } = context.__api;

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
      problems.push(`${page.id}: isleme sirasinda hata — ${error.message}`);
      continue;
    }

    totalHeadings += out.headings.length;
    totalHtml += out.html.length;

    if (out.html.trim().length < 40) problems.push(`${page.id}: cikti bos`);

    // Etiket dengesi: acilan her blok etiketi kapanmali.
    for (const tag of ['div', 'table', 'pre', 'code', 'ul', 'ol', 'li', 'blockquote', 'p', 'tr', 'td', 'th']) {
      const open = (out.html.match(new RegExp('<' + tag + '(?=[\\s>])', 'g')) ?? []).length;
      const close = (out.html.match(new RegExp('</' + tag + '>', 'g')) ?? []).length;
      if (open !== close) problems.push(`${page.id}: <${tag}> dengesiz — ${open} acilis, ${close} kapanis`);
    }

    // Isaretlemenin metne sizmasi: govdede kalan markdown izleri.
    const leaked = out.html.match(/^(#{2,6} |\| )/m);
    if (leaked) problems.push(`${page.id}: islenmemis markdown kalmis — ${JSON.stringify(leaked[0])}`);

    // Baglantilar: depo icindeki .md yollari hash rotasina cevrilmis olmali.
    const mdLink = out.html.match(/href="(?!https?:)[^"]*\.md[^"]*"/);
    if (mdLink) problems.push(`${page.id}: cevrilmemis dokuman baglantisi — ${mdLink[0]}`);
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
  `${(totalHtml / 1024).toFixed(0)} KB HTML uretildi — sorun yok`
);
