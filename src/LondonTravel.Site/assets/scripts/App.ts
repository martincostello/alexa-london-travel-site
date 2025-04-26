// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { Banner } from './Banner';
import { Manage } from './Manage';
import { Preferences } from './Preferences';
import { SwaggerUI } from './SwaggerUI';
import { Tracking } from './Tracking';

export class App {
    constructor() {}

    initialize() {
        this.updateBanner();
        this.loadImages();
        this.setupServiceWorker();

        this.configureAnalytics();

        Manage.initialize();

        const preferences = new Preferences();
        preferences.initialize();

        Banner.show();

        SwaggerUI.configure();
    }

    private configureAnalytics() {
        document.querySelectorAll('a, button, input, .ga-track-click').forEach((element) => {
            element.addEventListener('click', (e) => {
                const label = element.getAttribute('data-ga-label') || element.getAttribute('id');
                if (label) {
                    const category = element.getAttribute('data-ga-category') || 'General';
                    const action = element.getAttribute('data-ga-action') || 'clicked';

                    Tracking.track(category, action, label);
                }
            });
        });
    }

    private loadImages() {
        setTimeout(() => {
            const images = document.querySelectorAll('img.lazy');
            images.forEach((image) => {
                let url = image.getAttribute('data-original');
                url = encodeURI(url);
                image.setAttribute('src', url);
                image.removeAttribute('data-original');
            });
        }, 500);
    }

    private setupServiceWorker() {
        if ('serviceWorker' in navigator) {
            (navigator as any).serviceWorker
                .register('/service-worker.js')
                .then(() => {})
                .catch((err: any) => {
                    console.warn('Failed to register Service Worker: ', err);
                });
        }
    }

    private updateBanner() {
        const element = document.getElementById('build-date');
        const locale = document.querySelector('meta[name="x-request-locale"]').getAttribute('content');

        moment.locale(locale);

        if (element) {
            const timestamp = element.getAttribute('data-timestamp');
            const format = element.getAttribute('data-format');
            const value = moment(timestamp, format);
            if (value.isValid()) {
                const text: string = value.fromNow();
                element.textContent = `| Last updated ${text}`;
            }
        }
    }
}
