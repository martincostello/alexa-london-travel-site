// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.londonTravel {

    /**
     * Represents the class for debug information.
     */
    export class Debug {

        /**
         * Gets the branch the website was built from.
         * @returns {string} The branch the website was built from.
         */
        public static branch(): string {
            return $("meta[name='x-site-branch']").attr("content");
        }

        /**
         * Gets the Git SHA the website was built from.
         * @returns {string} The Git SHA the website was built from.
         */
        public static revision(): string {
            return $("meta[name='x-site-revision']").attr("content");
        }

        /**
         * Logs a message.
         * @param {string} message - The optional message to log.
         * @param {any} [optionalParams] - The optional parameters to log.
         */
        public static log(message: string, optionalParams?: any): void {
            console.log(message, optionalParams);
        }
    }
}
