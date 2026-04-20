import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useLocation } from 'react-router-dom';
import './AccommodationSearch.css';

const AccommodationSearch = () => {
    const { t, i18n } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();

    // Állapotok
    const [accommodations, setAccommodations] = useState([]);
    const [filteredAccommodations, setFilteredAccommodations] = useState([]);
    const [amenities, setAmenities] = useState([]);
    const [selectedAmenities, setSelectedAmenities] = useState([]);
    const [cityFilter, setCityFilter] = useState('');
    const [priceRange, setPriceRange] = useState(50000); 
    const [selectedTypes, setSelectedTypes] = useState([]);
    const [isBikeFriendly, setIsBikeFriendly] = useState(false);
    const [minRating, setMinRating] = useState(0);
    const [sortOrder, setSortOrder] = useState('recommended');

    // Segédfüggvény az átlagoláshoz
    const getAverageRating = (ratings) => {
        if (!ratings || ratings.length === 0) return 0;
        const sum = ratings.reduce((acc, curr) => acc + curr.pont, 0);
        return parseFloat((sum / ratings.length).toFixed(1));
    };

    // Adatok betöltése komponens indulásakor
    useEffect(() => {
        // Szállások lekérése
        fetch('https://localhost:7284/api/szallas') 
            .then(res => res.json())
            .then(data => {
                setAccommodations(data);
                setFilteredAccommodations(data);
            })
            .catch(err => console.error("Hiba a szállásoknál:", err));

        // Szolgáltatások lekérése
        fetch('https://localhost:7284/api/szolgaltatasok') 
            .then(res => res.json())
            .then(data => setAmenities(data))
            .catch(err => console.error("Hiba a szolgáltatásoknál:", err));
    }, []);

    // Város szűrő kinyerése az URL-ből (ha a főoldalról érkezik a júzer)
    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const city = params.get('city');
        if (city) setCityFilter(city);
    }, [location]);

    // Kombinált szűrési logika
    useEffect(() => {
        let result = [...accommodations];
        
        // 1. Város/Cím szűrő (JAVÍTVA: szallasCime helyett nev-et keresünk)
        if (cityFilter) {
            result = result.filter(item => 
                (item.nev?.toLowerCase().includes(cityFilter.toLowerCase())) ||
                (item.telepules?.toLowerCase().includes(cityFilter.toLowerCase()))
            );
        }

        // 2. Ár szűrő
        result = result.filter(item => item.ar <= priceRange);
        
        // 5. Dinamikus szolgáltatások szűrő (Minden kijelöltnek meg kell lennie)
        if (selectedAmenities.length > 0) {
            result = result.filter(item => 
                selectedAmenities.every(selectedId => 
                    item.szallasSzolgaltatasok?.some(s => (s.szoid || s.szolgId) === selectedId)
                )
            );
        }

        // 6. Értékelés szűrő
        if (minRating > 0) {
            result = result.filter(item => getAverageRating(item.ertekelesek) >= minRating);
        }

        // Rendezés
        if (sortOrder === 'cheapest') {
            result.sort((a, b) => a.ar - b.ar);
        } else if (sortOrder === 'popular') {
            result.sort((a, b) => getAverageRating(b.ertekelesek) - getAverageRating(a.ertekelesek));
        }

        setFilteredAccommodations(result);
    }, [priceRange, selectedTypes, isBikeFriendly, minRating, sortOrder, accommodations, selectedAmenities, cityFilter]);

    // Handlerek
    const handleTypeChange = (type) => {
        setSelectedTypes(prev => prev.includes(type) ? prev.filter(t => t !== type) : [...prev, type]);
    };

    const handleAmenityChange = (id) => {
        setSelectedAmenities(prev => prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]);
    };

    const clearFilters = () => {
        setPriceRange(50000);
        setSelectedTypes([]);
        setSelectedAmenities([]);
        setIsBikeFriendly(false);
        setMinRating(0);
        setSortOrder('recommended');
        setCityFilter('');
    };

    const formatPrice = (num) => {
        return num?.toLocaleString(i18n.language === 'hu' ? 'hu-HU' : 'en-US');
    };

    return (
        <div className="search-page">
            <div className="search-page-container">
                <aside className="sidebar">
                    <div className="sidebar-header">
                        <h3>{t('search_page.sidebar_title')}</h3>
                        <button className="btn-clear" onClick={clearFilters}>{t('search_page.clear_all')}</button>
                    </div>

                    <div className="filter-group">
                        <label>{t('search_page.price_label')}</label>
                        <input 
                            type="range" min="0" max="50000" step="1000"
                            className="price-slider"
                            value={priceRange} 
                            onChange={(e) => setPriceRange(Number(e.target.value))} 
                        />
                        <div className="price-labels">
                            <span>0 Ft</span>
                            <span>{formatPrice(priceRange)} Ft</span>
                        </div>
                    </div>

                    <div className="filter-group">
                        <label>{t('search_page.amenities_label')}</label>
                        {amenities.map(amenity => (
                            <label key={amenity.szoid} className="check-item">
                                <input type="checkbox" checked={selectedAmenities.includes(amenity.szoid)} onChange={() => handleAmenityChange(amenity.szoid)} />
                                <span className="checkmark"></span>
                                <span className="label-text">{amenity.nev}</span>
                            </label>
                        ))}
                    </div>

                    <div className="filter-group">
                        <label>{t('search_page.min_rating')}</label>
                        <div className="rating-buttons">
                            {[3, 4, 4.5].map(val => (
                                <button key={val} className={`rating-btn ${minRating === val ? 'active' : ''}`} onClick={() => setMinRating(minRating === val ? 0 : val)}>
                                    {val}+
                                </button>
                            ))}
                        </div>
                    </div>
                </aside>

                <main className="results-area">
                    <div className="results-header">
                        <div className="header-left">
                            <h2>{cityFilter ? `${t('search_page.search_label')}: "${cityFilter}"` : t('search_page.results_title')}</h2>
                            <span className="results-count">({filteredAccommodations.length} {t('search_page.results_count')})</span>
                            {cityFilter && (
                                <button className="btn-show-all" onClick={() => setCityFilter('')}>✕ {t('search_page.show_all_accommodations') || 'Összes mutatása'}</button>
                            )}
                        </div>
                        <div className="header-right">
                            <span className="sort-label">{t('search_page.sort_label')}</span>
                            <select className="sort-select" value={sortOrder} onChange={(e) => setSortOrder(e.target.value)}>
                                <option value="recommended">{t('search_page.sort_options.recommended')}</option>
                                <option value="cheapest">{t('search_page.sort_options.cheapest')}</option>
                                <option value="popular">{t('search_page.sort_options.popular')}</option>
                            </select>
                        </div>
                    </div>

                    <div className="accommodation-grid">
                        {filteredAccommodations.length > 0 ? (
                            filteredAccommodations.map(item => (
                                <div key={item.szid} className="hotel-card">
                                    <div className="card-image">
                                        {/* JAVÍTVA: Kép alt tagje */}
                                        <img 
                                            src={item.szallaskep ? `https://localhost:7284/${item.szallaskep}` : 'https://via.placeholder.com/600x400?text=Nincs+kép'} 
                                            alt={item.nev} 
                                        />
                                        {item.kerekparosBarat && <span className="badge">{t('search_page.bike_friendly').toUpperCase()}</span>}
                                        <span className="rating-tag">
                                            ★ {getAverageRating(item.ertekelesek) > 0 ? getAverageRating(item.ertekelesek) : 'Új'}
                                            {item.ertekelesek?.length > 0 && ` (${item.ertekelesek.length})`}
                                        </span>
                                    </div>
                                    <div className="card-body">
                                        {/* JAVÍTVA: szallasCime helyett nev */}
                                        <h4>{item.nev || t('search_page.no_name', 'Nincs név megadva')}</h4>
                                        <p className="loc">📍 {item.telepules || t('search_page.default_location')}</p>
                                        <div className="card-footer">
                                            <div className="price-info">
                                                <span className="price-val">{formatPrice(item.ar)} Ft</span>
                                                <span className="price-unit">/ {i18n.language === 'hu' ? 'éj' : 'night'}</span>
                                            </div>
                                            <button 
                                                className="btn-details"
                                                onClick={() => navigate(`/accommodation/${item.szid}`)}
                                            >
                                                {t('search_page.booking_btn')}
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))
                        ) : (
                            <div className="no-results">{t('search_page.no_results')}</div>
                        )}
                    </div>
                </main>
            </div>
        </div>
    );
};

export default AccommodationSearch;