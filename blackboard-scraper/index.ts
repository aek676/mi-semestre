import { chromium } from "playwright";

(async () => {
    const browser = await chromium.launch({ headless: false });
    console.log("!Corriendo con Bun!");
})();