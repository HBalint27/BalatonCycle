import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import your translation files (we will create these next)
import translationEN from './locales/en.json';
import translationHU from './locales/hu.json';

const resources = {
  en: {
    translation: translationEN
  },
  hu: {
    translation: translationHU
  }
};

i18n
  .use(LanguageDetector) // Detects user language
  .use(initReactI18next) // Passes i18n down to react-i18next
  .init({
    resources,
    fallbackLng: 'hu', // Default language
    interpolation: {
      escapeValue: false // React already escapes values
    }
  });

export default i18n;