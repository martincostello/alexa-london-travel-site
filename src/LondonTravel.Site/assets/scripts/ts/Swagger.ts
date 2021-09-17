// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

declare var SwaggerUIBundle: any;
declare var SwaggerUIStandalonePreset: any;

function HideTopbarPlugin(): any {
    return {
        components: {
            Topbar: (): any => {
                return null;
            }
        }
    };
}

window.onload = () => {
    if ("SwaggerUIBundle" in window) {
        const url: string = $("link[rel='swagger']").attr("href");
        const ui: any = SwaggerUIBundle({
            url: url,
            dom_id: "#swagger-ui",
            deepLinking: true,
            presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIStandalonePreset
            ],
            plugins: [
                SwaggerUIBundle.plugins.DownloadUrl,
                HideTopbarPlugin
            ],
            layout: "StandaloneLayout",
            booleanValues: ["false", "true"],
            defaultModelRendering: "schema",
            displayRequestDuration: true,
            jsonEditor: true,
            showRequestHeaders: true,
            supportedSubmitMethods: ["get"],
            tryItOutEnabled: true,
            validatorUrl: null,
            responseInterceptor: (response: any): any => {
                // Delete overly-verbose headers from the UI
                delete response.headers["content-security-policy"];
                delete response.headers["content-security-policy-report-only"];
                delete response.headers["feature-policy"];
            }
        });

        (window as any).ui = ui;
    }
};
