interface IVenderEvents {
    hidden: string;
    event: string;
    state: string;
}

const vendorEvents: IVenderEvents[] = [
    {
        hidden: 'hidden',
        event: 'visibilitychange',
        state: 'visibilityState',
    },
    {
        hidden: 'webkitHidden',
        event: 'webkitvisibilitychange',
        state: 'webkitVisibilityState',
    },
    {
        hidden: 'mozHidden',
        event: 'mozvisibilitychange',
        state: 'mozVisibilityState',
    },
    {
        hidden: 'msHidden',
        event: 'msvisibilitychange',
        state: 'msVisibilityState',
    },
    {
        hidden: 'oHidden',
        event: 'ovisibilitychange',
        state: 'oVisibilityState',
    },
];

const hasDocument = typeof document !== 'undefined';
export const isSupported = hasDocument && Boolean(document.addEventListener);

export const vendorEvent = ((): IVenderEvents | null => {
    if (!isSupported) {
        return null;
    }
    for (let event of vendorEvents) {
        if (event.hidden in document) {
            return event;
        }
    }
    // otherwise it's not supported
    return null;
})();

export const isSupportedLocal = isSupported && vendorEvent;

export const isVisible = () => {
    if (!vendorEvent) {
        return true;
    }

    const hidden = vendorEvent.hidden as keyof Document;
    const currentlyVisible = !document[hidden];
    return currentlyVisible;
};
