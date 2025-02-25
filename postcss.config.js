export default (ctx)  => ({
  plugins: {
       'postcss-import': {},
       cssnano: process.env.NODE_ENV === 'production' ? {} : false,
       "@tailwindcss/postcss": {},
       autoprefixer: {},
  },
})
