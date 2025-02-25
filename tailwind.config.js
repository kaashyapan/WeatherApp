/** @type {import('tailwindcss').Config} */
export default {
  content: ["./templates/*.{fs}", "./templates/shared/*.{fs}"],
  theme: {
    extend: {},
  },
  plugins: [require("@tailwindcss/forms"), require("@tailwindcss/typography")]
}
