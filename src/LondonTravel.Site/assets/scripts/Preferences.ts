// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export class Preferences {
    private clearButton: HTMLInputElement;
    private container: HTMLElement;
    private favoritesCount: HTMLElement;
    private initialState: string;
    private otherCount: HTMLElement;
    private resetButton: HTMLInputElement;
    private saveButton: HTMLInputElement;

    public constructor() {}

    initialize() {
        this.container = document.querySelector('.js-preferences-container');

        if (!this.container) {
            return;
        }

        this.initialState = this.getCurrentState();

        this.clearButton = this.container.querySelector('.js-preferences-clear');
        this.resetButton = this.container.querySelector('.js-preferences-reset');
        this.saveButton = this.container.querySelector('.js-preferences-save');

        this.favoritesCount = this.container.querySelector('.js-favorites-count');

        if (this.favoritesCount) {
            this.favoritesCount.classList.remove('d-none');
        }

        this.otherCount = this.container.querySelector('.js-other-count');

        if (this.otherCount) {
            this.otherCount.classList.remove('d-none');
        }

        // Treat entire div as a button
        const lines = this.container.querySelectorAll('.js-travel-line');

        lines.forEach((element) => {
            element.addEventListener('click', () => {
                const selector = 'input[type="checkbox"]';
                let checkbox = element.querySelector<HTMLInputElement>(selector);
                if (!checkbox) {
                    checkbox = element.parentElement.querySelector<HTMLInputElement>(selector);
                }

                if (element.tagName.toUpperCase() === 'LI') {
                    checkbox.checked = !checkbox.checked;
                }

                this.checkCurrentState();
            });
        });

        // Show the reset button to restore initial preferences' state
        this.resetButton.addEventListener('click', () => {
            this.resetFavoriteLines();
        });
        this.resetButton.classList.add('disabled');
        this.resetButton.classList.remove('d-none');
        this.resetButton.disabled = true;

        // Disable the clear button if everything is currently deselected
        if (this.initialState === '') {
            this.clearButton.classList.add('disabled');
            this.clearButton.disabled = true;
        }

        // Add handler for clearing the selections and show the button
        this.clearButton.addEventListener('click', () => {
            this.getAllCheckboxes().forEach((line) => {
                line.checked = false;
            });
            this.checkCurrentState();
        });
        this.clearButton.classList.remove('d-none');

        // Disable the save button the state is different
        this.saveButton.classList.add('disabled');
        this.saveButton.disabled = true;

        // Check the form's state whenever an option is changed
        this.container.querySelectorAll('.js-line-preference').forEach((element) => {
            element.addEventListener('change', () => {
                this.checkCurrentState();
            });
        });

        // Dismiss any visible success alerts
        setTimeout(() => {
            if ('$' in window) {
                const jq = window['$'] as any;
                jq(document.querySelector('.alert-success')).slideUp(1000);
            }
        }, 3000);
    }

    private checkCurrentState() {
        const currentState: string = this.getCurrentState();
        const isDirty: boolean = this.initialState !== currentState;

        this.setButtonState(this.resetButton, isDirty);
        this.setButtonState(this.saveButton, isDirty);
        this.setButtonState(this.clearButton, currentState !== '');

        if (this.otherCount) {
            const total: number = this.getAllCheckboxes().length;
            const favorites: number = this.getSelectedCheckboxes().length;
            const others: number = total - favorites;

            if (this.favoritesCount) {
                this.favoritesCount.textContent = `(${favorites.toString(10)})`;
            }

            this.otherCount.textContent = `(${others.toString(10)})`;
        }
    }

    private getAllCheckboxes() {
        return this.container.querySelectorAll<HTMLInputElement>('input[type="checkbox"]');
    }

    private getCurrentState() {
        const ids: string[] = [];

        this.getSelectedCheckboxes().forEach((element) => {
            ids.push(element.getAttribute('data-line-id'));
        });

        ids.sort();

        return ids.join();
    }

    private getSelectedCheckboxes() {
        return this.container.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:checked');
    }

    private resetFavoriteLines() {
        const ids: string[] = this.initialState.split(',');

        this.getAllCheckboxes().forEach((element) => {
            const id: string = element.getAttribute('data-line-id');
            element.checked = ids.indexOf(id) > -1;
        });

        this.checkCurrentState();
    }

    private setButtonState(button: HTMLInputElement, enabled: boolean) {
        if (enabled) {
            button.disabled = false;
            button.classList.remove('disabled');
        } else {
            button.disabled = true;
            button.classList.add('disabled');
        }
    }
}
