// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        environment: 'jsdom',
        globals: true,
        coverage: {
            provider: 'v8',
            reporter: ['html', 'lcov', 'text'],
            exclude: ['node_modules/', 'wwwroot/', '*.config.{js,ts,cjs,mjs}'],
        },
    },
});
