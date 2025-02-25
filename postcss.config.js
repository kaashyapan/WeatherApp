console.log(process.env.ENV)
module.exports = (ctx) => ({
  plugins: {
       'postcss-import': {},
       cssnano: process.env.ENV === 'prod' ? {} : false,
       "@tailwindcss/postcss": {},
       autoprefixer: {},
  },
})
