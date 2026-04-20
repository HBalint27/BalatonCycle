import React from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import './Footer.css';

const Footer = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <footer className="footer">
      <div className="footer-container">
        <div className="footer-brand">
          <div className="logo" onClick={() => navigate('/')}>
            <span className="logo-text"><strong>BALATON</strong>CYCLE</span>
          </div>
          <p>{t('hero.subtitle')}</p>
        </div>

        <div className="footer-links">
          <h4>{t('footer.links_title')}</h4>
          <ul>
            <li onClick={() => navigate('/')}>{t('footer.main-page')}</li>
            <li onClick={() => navigate('/szallasok')}>{t('footer.accommodations')}</li>
            <li>{t('footer.contact')}</li>
          </ul>
        </div>

        <div className="footer-contact">
          <h4>{t('footer.contact')}</h4>
          <p>Email: balatoncycle@gmail.com</p>
          <p>Tel: +36 30 123 4567</p>
        </div>
      </div>
      
      <div className="footer-bottom">
        <p>{t('footer.rights')}</p>
      </div>
    </footer>
  );
};

export default Footer;