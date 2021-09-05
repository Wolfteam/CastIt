import { useEffect, useState } from 'react';

import { isSupportedLocal, isVisible, vendorEvent } from '../utils/visibility';

const usePageVisibility = () => {
    const initiallyVisible = isVisible();
    const [state, setState] = useState(initiallyVisible);

    useEffect(() => {
        if (isSupportedLocal) {
            const handler = () => {
                const currentlyVisible = isVisible();
                setState(currentlyVisible);
            };

            document.addEventListener(vendorEvent!.event, handler);

            return () => {
                document.removeEventListener(vendorEvent!.event, handler);
            };
        }
    }, []);

    return state;
};

export default usePageVisibility;
