import { defineConfig } from "rolldown";

export default defineConfig([
	{
		// define: { "process.env.NODE_ENV": "'production'" },
		input: "js/index.js",
		output: {
			format: "es",
			file: "wwwroot/app.min.js",
			minify: process.env.NODE_ENV === "production" ? true : false,
		},
	},
]);
