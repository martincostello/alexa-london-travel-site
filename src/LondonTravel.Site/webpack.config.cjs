const path = require('path');
const webpack = require('webpack');
const cssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const removeEmptyScriptsPlugin = require('webpack-remove-empty-scripts');

module.exports = {
    devtool: 'source-map',
    entry: {
        css: path.resolve(__dirname, './assets/styles/main.css'),
        js: path.resolve(__dirname, './assets/scripts/main.ts'),
    },
    mode: 'production',
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    optimization: {
        minimize: true,
        minimizer: [
            '...',
            new cssMinimizerPlugin(),
        ],
    },
    output: {
        cssFilename: '[name]/main.css',
        clean: true,
        filename: '[name]/main.js',
        path: path.resolve(__dirname, 'wwwroot', 'assets'),
    },
    plugins: [
        new removeEmptyScriptsPlugin(),
        new webpack.ContextReplacementPlugin(/moment[/\\]locale$/, /en-gb/),
    ],
    resolve: {
        extensions: ['.css', '.tsx', '.ts', '.js'],
    },
};
