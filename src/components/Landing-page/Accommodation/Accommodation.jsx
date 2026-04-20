import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './Accommodation.css';

const Accommodation = () => {
    const { t, i18n } = useTranslation();
    const [accommodations, setAccommodations] = useState([]);
    const [scroll, setScroll] = useState(0);
    const sliderRef = useRef(null);
    const navigate = useNavigate();

    // Segédfüggvény az átlagoláshoz (Dinamikus adatfeldolgozás)
    const getAverageRating = (ratings) => {
        if (!ratings || ratings.length === 0) return 0;
        const sum = ratings.reduce((acc, curr) => acc + curr.pont, 0);
        return (sum / ratings.length).toFixed(1);
    };

    const getImageUrl = (imagePath) => {
        if (!imagePath) return 'https://via.placeholder.com/600x400?text=Nincs+kép';
        return `https://localhost:7284/${imagePath}`;
    };

    useEffect(() => {
        fetch('https://localhost:7284/api/szallas') 
            .then(res => res.json())
            .then(data => {
                setAccommodations(data);
            })
            .catch(err => {
                console.error("Hiba az adatok lekérésekor:", err);
            });
    }, []);

    const handleScroll = (direction) => {
        const container = sliderRef.current;
        if (!container || accommodations.length === 0) return;

        const card = container.querySelector('.card');
        if (!card) return;

        const cardWidth = card.offsetWidth + 24; // kártya szélessége + gap
        const maxScroll = -(cardWidth * (accommodations.length - 1));

        if (direction === 'next') {
            setScroll(prev => (prev > maxScroll ? prev - cardWidth : prev));
        } else {
            setScroll(prev => (prev < 0 ? prev + cardWidth : 0));
        }
    };

    return (
        <section id="accommodations" className="accommodations container">
            <div className="section-title-row">
                <div className="title-group">
                    <h2>{t('slider.title')}</h2>
                    <p className="subtitle">🛡️ {t('slider.subtitle')}</p>
                </div>
                <div className="slider-arrows">
                    <button className="arrow" onClick={() => handleScroll('prev')}>‹</button>
                    <button className="arrow" onClick={() => handleScroll('next')}>›</button>
                </div>
            </div>

            <div className="slider-viewport">
                <div 
                    ref={sliderRef}
                    className="accommodation-grid" 
                    style={{ 
                        transform: `translateX(${scroll}px)`,
                        transition: 'transform 0.5s cubic-bezier(0.25, 0.46, 0.45, 0.94)',
                        display: 'flex' 
                    }}
                >
                    {accommodations.map((item) => (
                        <div 
                            className="card" 
                            key={item.szid} 
                            /* Navigáció hozzáadása a szállás ID-ja alapján */
                            onClick={() => navigate(`/accommodation/${item.szid}`)} 
                            style={{ cursor: 'pointer' }} // Hogy látsszon, hogy kattintható
                        >
                            <div 
                                className="card-img" 
                                style={{ 
                                    backgroundImage: `url(${getImageUrl(item.szallaskep)})`,
                                    backgroundSize: 'cover',
                                    backgroundPosition: 'center',
                                    height: '220px' 
                                }}
                            >
                                <span className="rating">
                                    ★ {getAverageRating(item.ertekelesek) > 0 
                                        ? `${getAverageRating(item.ertekelesek)} (${item.ertekelesek.length})` 
                                        : 'Új'}
                                </span>
                            </div>
                            <div className="card-info">
                                {/* JAVÍTVA: szallasCime cserélve nev-re */}
                                <h3>{item.nev || t('slider.no_name')}</h3>
                                <p className="location">📍 {item.telepules || t('slider.location_area')}</p>
                                <p className="price">
                                    <strong>
                                        {item.ar 
                                            ? `${item.ar.toLocaleString(i18n.language === 'hu' ? 'hu-HU' : 'en-US')} Ft ` 
                                            : ''} 
                                        {t('slider.price_per_night')}
                                    </strong>
                                </p>
                            </div>
                        </div>
                    ))}
                </div>
                
                {accommodations.length === 0 && (
                    <div style={{ textAlign: 'center', padding: '60px 0', color: '#8a817c' }}>
                        <p>{t('slider.no_results')}</p>
                    </div>
                )}

                <div className="view-all-container">
                    <button 
                        className="btn-view-all" 
                        onClick={() => navigate('/szallasok')}
                    >
                        {t('slider.view_all')}
                    </button>
                </div>
            </div>
        </section>
    );
};

export default Accommodation;