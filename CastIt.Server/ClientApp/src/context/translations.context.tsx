import { useState, createContext, Dispatch, SetStateAction } from 'react';
import { AppLanguage } from '../enums';
import translations from '../services/translations';

interface ITranslationContext {
    currentLanguage: string;
}

const langKey = 'castit.lang';

const saveLanguageToStorage = (val: string): void => localStorage.setItem(langKey, val);
const getLanguageFromStorage = (): string | null => localStorage.getItem(langKey);

const supportedLangs: string[] = ['es', 'en'];
export const getLanguageString = (lang: number): string => {
    const key = AppLanguage[lang];
    const theEnum: AppLanguage = AppLanguage[key as keyof typeof AppLanguage];
    switch (theEnum) {
        case AppLanguage.english:
            return supportedLangs[1];
        default:
            return supportedLangs[0];
    }
};

export const getLanguageEnum = (lang: string): AppLanguage => {
    switch (lang.toLowerCase()) {
        case supportedLangs[0]:
            return AppLanguage.spanish;
        default:
            return AppLanguage.english;
    }
};

export const TranslationContext = createContext<[ITranslationContext | null, Dispatch<SetStateAction<ITranslationContext>> | null]>([
    null,
    null,
]);

export const TranslationContextProvider = (children: any) => {
    let lang = getLanguageFromStorage();
    if (!lang || !supportedLangs.includes(lang)) {
        lang = supportedLangs[1];
    }

    const [trans, setTrans] = useState<ITranslationContext>({
        currentLanguage: lang,
    });

    if (trans.currentLanguage !== translations.getLanguage()) {
        translations.setLanguage(trans.currentLanguage);
    }

    saveLanguageToStorage(trans.currentLanguage);

    return <TranslationContext.Provider value={[trans, setTrans]}>{children.children}</TranslationContext.Provider>;
};
