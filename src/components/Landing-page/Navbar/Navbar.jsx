import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { jwtDecode } from "jwt-decode";
import { useTranslation } from 'react-i18next';
import './Navbar.css';

const Navbar = () => {
    const { t, i18n } = useTranslation();
    const [isActive, setIsActive] = useState(false);
    const [user, setUser] = useState(null);
    const [searchTerm, setSearchTerm] = useState(''); 
    const navigate = useNavigate();

    // Felhasználó ellenőrzése token alapján
    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decoded = jwtDecode(token);
                setUser({ email: decoded["unique_name"] || decoded["name"] });
            } catch (error) {
                console.error("Érvénytelen token");
                localStorage.removeItem('token');
            }
        }
    }, []);

    const handleLogout = () => {
        localStorage.removeItem('token');
        setUser(null);
        navigate('/');
    };

    // Keresés Enter leütésre
    const handleSearch = (e) => {
        if (e.key === 'Enter' && searchTerm.trim() !== '') {
            navigate(`/szallasok?city=${encodeURIComponent(searchTerm.trim())}`);
            setSearchTerm(''); 
            setIsActive(false); 
        }
    };

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
        <header className="navbar">
            <div className="container nav-content">
                {/* LOGO */}
                <div className="logo" onClick={() => navigate('/')}>
                    <svg viewBox="0 0 100 60" className="bike-logo-svg">
                        <path
                            d="M20,45 C20,38 26,32 33,32 C40,32 46,38 46,45 C46,52 40,58 33,58 C26,58 20,52 20,45 Z 
                            M70,45 C70,38 76,32 83,32 C90,32 96,38 96,45 C96,52 90,58 83,58 C76,58 70,52 70,45 Z
                            M33,45 L50,45 L65,25 L83,45
                            M65,25 L75,25 M75,25 L80,15
                            M50,45 L45,30 L55,30"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="3.5"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        />
                    </svg>
                    <span className="logo-text"><strong>BALATON</strong>CYCLE</span>
                </div>

                {/* Mobil menü gomb */}
                <button className="menu-toggle" onClick={() => setIsActive(!isActive)}>
                    <span></span><span></span><span></span>
                </button>

                <div className={`nav-right ${isActive ? 'active' : ''}`}>
                    {/* KERESŐ */}
                    <div className="search-container">
                        <span className="search-icon">🔍</span>
                        <input 
                            type="text" 
                            placeholder={t('navbar.search_placeholder')} 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            onKeyDown={handleSearch}
                        />
                    </div>

                    {/* BELÉPÉS / PROFIL */}
                    {!user ? (
                        <Link to="/login" className="nav-link">{t('navbar.login')}</Link>
                    ) : (
                        <div className="profile-dropdown">
                            <div className="profile-trigger">
                                <div className="profile-avatar">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="user-icon">
                                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                                        <circle cx="12" cy="7" r="4"></circle>
                                    </svg>
                                </div>
                            </div>
                            <div className="dropdown-menu">
                                <Link to="/profile?tab=adatok">{t('navbar.profile.data')}</Link>
                                <Link to="/profile?tab=hirdeteseim">{t('navbar.profile.my_ads')}</Link>
                                <Link to="/profile?tab=foglalasok">{t('navbar.profile.my_bookings')}</Link>
                                <hr />
                                <button onClick={handleLogout} className="logout-btn">
                                    {t('navbar.profile.logout')}
                                </button>
                            </div>
                        </div>
                    )}

                    {/* HIRDETÉSFELADÁS */}
                    <button className="btn-post" onClick={handlePostClick}>
                        <span>⊕</span> {t('navbar.post_ad')}
                    </button>

                    {/* NYELVVÁLASZTÓ */}
                    <div className="lang-selector">
                        <button 
                            className={`lang-btn ${i18n.language === 'hu' ? 'active' : ''}`}
                            onClick={() => i18n.changeLanguage('hu')}
                        >HU</button>
                        <span className="lang-divider">|</span>
                        <button 
                            className={`lang-btn ${i18n.language === 'en' ? 'active' : ''}`}
                            onClick={() => i18n.changeLanguage('en')}
                        >EN</button>
                    </div>
                </div>
            </div>
        </header>
    );
};

export default Navbar;