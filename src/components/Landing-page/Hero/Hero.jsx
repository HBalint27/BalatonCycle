import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './Hero.css';

const Hero = () => {
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  const handlePostClick = () => {
    const token = localStorage.getItem('token');
    if (!token) {
        alert(t('common.alert_login_required'));
        navigate('/login');
    } else {
        navigate('/add-accommodation');
    }
  };

  return (
    <section className="hero">
      <div className="hero-overlay">
        <div className="hero-card">
          <h1>
            {/* Logic: If HU, highlight 'balatoni kalandba'. If EN, highlight 'Lake Balaton' */}
            {i18n.language === 'hu' ? (
              <>Vágj bele a <br /><span className="accent-text">balatoni kalandba!</span></>
            ) : (
              <>Start your adventure at <br /><span className="accent-text">Lake Balaton!</span></>
            )}
          </h1>
          
          <p>{t('hero.subtitle')}</p>
          
          <div className="hero-buttons">
            <button 
              className="btn-main" 
              onClick={() => navigate('/szallasok')}
            >
              {t('hero.btn_search')}
            </button>
            
            <button 
              className="btn-outline" 
              onClick={handlePostClick}
            >
              {t('navbar.post_ad')}
            </button>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Hero;