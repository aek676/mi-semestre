import { chromium } from "playwright";

(async () => {
    console.log("🚀 Iniciando...");

    // 1. Lanzamos el navegador
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext({
        userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36'
    });
    const page = await context.newPage();

    // 2. Navegamos a algún sitio para ver algo
    await page.goto("https://aulavirtual.ual.es");
    console.log("✅ !Corriendo con Bun y cargó Aula Virtual!");

    // 3. TRUCO: Esperar indefinidamente para que no se cierre
    console.log("⏸️  El navegador está abierto. Presiona CTRL+C en la terminal para cerrar.");

    // Esto mantiene el proceso vivo hasta que tú lo mates manualmente
    await new Promise(() => { });

    // (Esta línea nunca se ejecutará a menos que cambies la lógica de arriba)
    await browser.close();
})();