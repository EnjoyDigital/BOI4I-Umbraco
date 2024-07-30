const path = require('path'),
  webpack = require('webpack'),
  fs = require('fs'),
  glob = require("glob"),
  jsGlobPath = './common/scripts/{,pages}/*.js'; //JS folder

module.exports = {
  mode: 'production', //uglify, optimise, minify
  devtool: 'source-map', //produce source maps for debugging
  entry: glob.sync(jsGlobPath).reduce(function (obj, el) {
    obj[path.parse(el).name] = el;
    return obj
  }, {}),
  output: {
    filename: '[name].bundle.js',
    sourceMapFilename: '[name].bundle.map'
  },
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /(node_modules|bower_components)/,
        use: {
          loader: 'babel-loader'
        }
      }
    ]
  },
  resolve: {
    alias: {
      jquery: "jquery/src/jquery"
    }
  },
  //plugins: [
  //  new webpack.ProvidePlugin({
  //      $: "jquery",
  //      jQuery: "jquery"
  //  })
  //]
};