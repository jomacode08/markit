/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        retroyellow: {
          50: "#FFF9F0",
          100: "#FFF3E0",
          200: "#FFE7C2",
          300: "#FFDAA3",
          400: "#FFCA7A",
          500: "#FFB84C",
          600: "#FF9B05",
          700: "#E08700",
          800: "#BD7100",
          900: "#855000",
          950: "#663D00"
        },
        retropink: {
          50: "#FEF6FA",
          100: "#FDE8F2",
          200: "#FBD5E8",
          300: "#F9B9D9",
          400: "#F693C4",
          500: "#F266AB",
          600: "#EF4399",
          700: "#E6147D",
          800: "#C11069",
          900: "#880C4A",
          950: "#630836"
        },
        retropurple: {
          50: "#F9F3FC",
          100: "#F2E7F8",
          200: "#E5CFF2",
          300: "#D6B3EA",
          400: "#C28FE0",
          500: "#A459D1",
          600: "#9B47CC",
          700: "#8D35C0",
          800: "#752CA0",
          900: "#521F70",
          950: "#3B1650"
        },
        retroblue: {
          50: "#F2FCFD",
          100: "#E4F9FB",
          200: "#C0F2F6",
          300: "#9DEAF1",
          400: "#6CE0EA",
          500: "#2CD3E1",
          600: "#1DBFCD",
          700: "#1AAAB7",
          800: "#168D98",
          900: "#106870",
          950: "#0B464C"
        }
      },
      fontFamily: {
        sans: ["Cera Round Pro", "Sans-serif"]
      }
    },
  },
  plugins: [],
}
