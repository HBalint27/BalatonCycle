import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import './Newsletter.css';

const Newsletter = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      const response = await fetch('https://localhost:7284/api/Felhasznalo/subscribe', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email }) 
      });

      if (response.ok) {
        alert("Sikeres feliratkozás!");
        setEmail('');
      }
    } catch (error) {
      console.error("Hiba:", error);
    }
  };

  return (
    <section className="newsletter-section">
      <div className="newsletter-container">
        <div className="newsletter-content">
          <h2>{t('newsletter.title')}</h2>
          <p>{t('newsletter.subtitle')}</p>
        </div>
        
        <form className="newsletter-form" onSubmit={handleSubmit}>
          <input 
            type="email" 
            placeholder={t('newsletter.placeholder')} 
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required 
          />
          <button type="submit" className="btn-newsletter">
            {t('newsletter.button')}
          </button>
        </form>
      </div>
    </section>
  );
};

export default Newsletter;