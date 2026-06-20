/** @type {import('tailwindcss').Config} */
module.exports = {
  // Scan Razor components + the host page for class usage.
  content: [
    "./Components/**/*.razor",
    "./Components/**/*.razor.cs",
    "./wwwroot/index.html"
  ],
  theme: {
    extend: {
      colors: {
        // Design system from the provided mockups.
        cream: {
          DEFAULT: "#F4ECE0", // page background
          card: "#FFFFFF",
          line: "#EFE7D9",     // dividers / icon tiles
          border: "#E7DCC9"
        },
        brick: {
          DEFAULT: "#9E3328", // primary
          dark: "#8A2C22",
          light: "#B5483B"
        },
        blush: "#F2DAD5",      // avatar circles
        ink: "#1F2733",        // headings / primary text
        subtle: "#8A8378"      // muted secondary text
      },
      fontFamily: {
        serif: ['"Playfair Display"', "Georgia", "serif"],
        sans: ["Inter", "system-ui", "sans-serif"],
        urdu: ['"Noto Nastaliq Urdu"', "serif"]
      },
      boxShadow: {
        card: "0 2px 14px rgba(31, 39, 51, 0.06)",
        nav: "0 -2px 14px rgba(31, 39, 51, 0.05)"
      },
      borderRadius: {
        card: "1.25rem",
        tile: "0.9rem"
      }
    }
  },
  plugins: []
};
