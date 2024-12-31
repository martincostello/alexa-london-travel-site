// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export class SwaggerUI {
    static configure() {
        if ('SwaggerUIBundle' in window && 'SwaggerUIStandalonePreset' in window) {
            const swaggerUIBundle = window['SwaggerUIBundle'] as any;
            const swaggerUIStandalonePreset = window['SwaggerUIStandalonePreset'] as any;

            const element = document.querySelector('link[rel="swagger"]');
            if (!element) {
                return;
            }

            const url: string = element.getAttribute('href');
            const ui: any = swaggerUIBundle({
                url: url,
                /* eslint-disable @typescript-eslint/naming-convention */
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [swaggerUIBundle.presets.apis, swaggerUIStandalonePreset],
                plugins: [
                    swaggerUIBundle.plugins.DownloadUrl,
                    (): any => {
                        return {
                            components: {
                                /* eslint-disable @typescript-eslint/naming-convention */
                                Topbar: (): any => null,
                            },
                        };
                    },
                ],
                layout: 'StandaloneLayout',
                booleanValues: ['false', 'true'],
                defaultModelRendering: 'schema',
                displayRequestDuration: true,
                jsonEditor: true,
                showRequestHeaders: true,
                supportedSubmitMethods: ['get'],
                tryItOutEnabled: true,
                validatorUrl: null,
                responseInterceptor: (response: any): any => {
                    delete response.headers['content-security-policy'];
                    delete response.headers['content-security-policy-report-only'];
                    delete response.headers['cross-origin-embedder-policy'];
                    delete response.headers['cross-origin-opener-policy'];
                    delete response.headers['cross-origin-resource-policy'];
                    delete response.headers['permissions-policy'];
                },
            });

            (window as any).ui = ui;
        }
    }
}
