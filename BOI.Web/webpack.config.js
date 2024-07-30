const path = require('path'),
	webpack = require('webpack'),
	glob = require("glob"),
	UglifyJsPlugin = require('uglifyjs-webpack-plugin'),
	jsGlobPath = './assets/src/scripts/{,pages}/*.js';
const { VueLoaderPlugin } = require("vue-loader");
let entryPoints = glob.sync(jsGlobPath).reduce(function (obj, el) {
	obj[path.parse(el).name] = el;
	return obj
}, {});

module.exports = {
	mode: 'production', //uglify, optimise, minify
	devtool: 'source-map', //produce source maps for debugging
	entry: entryPoints,
	output: {
		filename: '[name].bundle.js',
		sourceMapFilename: '[name].bundle.map',
	},
	module: {
		rules: [
			{
				test: /\.js$/,
				exclude: /(node_modules|bower_components)/,
				use: {
					loader: 'babel-loader'
				}
			},
			{
				test: /\.vue$/,
				loader: "vue-loader",
			},
		],
	},
	resolve: {
		alias: {
			jquery: "jquery/src/jquery",
			vue$: "vue/dist/vue.runtime.esm.js",
		},
		extensions: ["*", ".js", ".vue", ".json"],
	},
	plugins: [
		new VueLoaderPlugin(),
	]
}