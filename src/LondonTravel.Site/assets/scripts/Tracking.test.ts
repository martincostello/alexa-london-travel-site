// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Tracking } from './Tracking';

describe('Tracking', () => {
    beforeEach(() => {
        // Clean up window.gtag before each test
        delete (window as any).gtag;
    });

    describe('track', () => {
        it('should return false when gtag is not available', () => {
            const result = Tracking.track('TestCategory', 'TestAction', 'TestLabel');
            expect(result).toBe(false);
        });

        it('should return true and call gtag when available', () => {
            const gtagMock = vi.fn();
            (window as any).gtag = gtagMock;

            const result = Tracking.track('TestCategory', 'TestAction', 'TestLabel');

            expect(result).toBe(true);
            expect(gtagMock).toHaveBeenCalledWith('event', 'TestAction', {
                /* eslint-disable @typescript-eslint/naming-convention */
                event_category: 'TestCategory',
                event_label: 'TestLabel',
                /* eslint-enable @typescript-eslint/naming-convention */
            });
        });

        it('should handle different categories and actions', () => {
            const gtagMock = vi.fn();
            (window as any).gtag = gtagMock;

            Tracking.track('Navigation', 'click', 'home-button');

            expect(gtagMock).toHaveBeenCalledWith('event', 'click', {
                /* eslint-disable @typescript-eslint/naming-convention */
                event_category: 'Navigation',
                event_label: 'home-button',
                /* eslint-enable @typescript-eslint/naming-convention */
            });
        });
    });
});
