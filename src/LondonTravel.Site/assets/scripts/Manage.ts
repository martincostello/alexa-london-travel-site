// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Represents the class for managing accounts.
 */
export class Manage {
    static initialize() {
        const hide = document.querySelector('.js-hidden-control');
        const form = document.querySelector<HTMLElement>('.js-modal-form');
        const modal = document.querySelector<HTMLElement>('.js-modal');

        if (hide) {
            hide.classList.remove('d-none');

            if ('$' in window) {
                const jq = window['$'] as any;
                jq(hide).fadeIn();
            }
        }

        if (form) {
            form.addEventListener('submit', () => {
                form.querySelector('.js-delete-control').classList.add('disabled');
                form.querySelector('.js-delete-content').classList.add('d-none');
                form.querySelector('.js-delete-loader').classList.remove('d-none');
            });
        }

        if (modal) {
            modal.addEventListener('show.bs.modal', () => {
                setTimeout(() => {
                    const confirmation = modal.querySelector<HTMLInputElement>('.js-modal-confirm');
                    confirmation.disabled = false;
                    confirmation.classList.remove('disabled');
                }, 3000);
            });
        }
    }
}
