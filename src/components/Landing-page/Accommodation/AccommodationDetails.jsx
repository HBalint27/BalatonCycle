import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import tourMapImage from '../../../assets/balaton_cycle.jpg'
import './AccommodationDetails.css';

const AccommodationDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [accommodation, setAccommodation] = useState(null);
    const [loading, setLoading] = useState(true);
    
    // We track the INDEX of the array instead of the URL string. null means closed.
    const [currentImageIndex, setCurrentImageIndex] = useState(null);
    // NEW: Track booking selections to pass to the payment page
    const [bookingDate, setBookingDate] = useState(new Date().toISOString().split('T')[0]);
    const [guestCount, setGuestCount] = useState("2 felnőtt");

    // HOOK 1: Fetch Data
    useEffect(() => {
        setLoading(true);
        fetch(`https://localhost:7284/api/szallas/${id}?ext=true`)
            .then(async res => {
                if (!res.ok) {
                    const errText = await res.text();
                    throw new Error(`Server Error ${res.status}: ${errText}`);
                }
                return res.json();
            })
            .then(data => {
                setAccommodation(data);
                setLoading(false);
            })
            .catch(err => {
                console.error("Backend hiba történt a lekérdezéskor:", err.message);
                setLoading(false);
            });
    }, [id]);

    // --- SAFELY CALCULATE IMAGES ABOVE EARLY RETURNS ---
    const mainImageUrl = accommodation?.szallaskep 
        ? `https://localhost:7284/${accommodation.szallaskep}` 
        : 'https://via.placeholder.com/1200x800?text=Nincs+kép';

    const galleryUrls = accommodation?.szobak?.length > 0
        ? accommodation.szobak.flatMap(szoba => szoba.kepek || []).map(kep => `https://localhost:7284/${kep.fajlnev}`)
        : [];

    const allImages = accommodation ? [mainImageUrl, ...galleryUrls, tourMapImage] : [];

    // --- SLIDER NAVIGATION LOGIC ---
    const handlePrevImage = useCallback((e) => {
        if (e) e.stopPropagation();
        setCurrentImageIndex(prev => (prev === 0 ? allImages.length - 1 : prev - 1));
    }, [allImages.length]);

    const handleNextImage = useCallback((e) => {
        if (e) e.stopPropagation();
        setCurrentImageIndex(prev => (prev === allImages.length - 1 ? 0 : prev + 1));
    }, [allImages.length]);

    // HOOK 2: Keyboard Navigation (MUST be above early returns!)
    useEffect(() => {
        const handleKeyDown = (e) => {
            if (currentImageIndex === null) return;
            if (e.key === 'ArrowLeft') handlePrevImage();
            if (e.key === 'ArrowRight') handleNextImage();
            if (e.key === 'Escape') setCurrentImageIndex(null);
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, [currentImageIndex, handlePrevImage, handleNextImage]);


    // --- EARLY RETURNS ---
    if (loading) return <div className="loader">Betöltés...</div>;
    if (!accommodation) return <div className="error">Szállás nem található.</div>;


    // --- RENDER LOGIC ---
    const reviews = accommodation.ertekelesek || [];
    const averageRating = reviews.length > 0 
        ? (reviews.reduce((acc, curr) => acc + (Number(curr.pont) || 0), 0) / reviews.length).toFixed(1)
        : "0.0";

    const mapQuery = encodeURIComponent(`${accommodation.telepules}, ${accommodation.utca} ${accommodation.hazszam}`);

    return (
        <div className="details-page">
            <main className="details-container">
                <div className="details-header">
                    <button className="back-link" onClick={() => navigate(-1)}>
                        ← Vissza a kereséshez
                    </button>
                    <h1>{accommodation.nev}</h1>
                    <p className="location-text">
                        <span className="material-symbols-outlined">location_on</span>
                        {accommodation.telepules}, {accommodation.utca} {accommodation.hazszam}
                    </p>
                </div>

                <section className="gallery-grid">
                    <div className="main-photo">
                        <img 
                            src={mainImageUrl} 
                            alt="Főkép" 
                            onClick={() => setCurrentImageIndex(0)}
                            className="clickable-image"
                        />
                    </div>
                    <div className="sub-photos">
                        {galleryUrls.length > 0 ? (
                            galleryUrls.map((url, index) => (
                                <img 
                                    key={index} 
                                    src={url} 
                                    alt={`Galéria kép ${index + 1}`} 
                                    onClick={() => setCurrentImageIndex(index + 1)}
                                    className="clickable-image"
                                    onError={(e) => { e.target.src = 'https://via.placeholder.com/200?text=Hiba'; }}
                                />
                            ))
                        ) : (
                            <p>Nincsenek elérhető képek a szobákhoz.</p>
                        )}
                    </div>
                </section>

                <div className="content-layout">
                    <div className="info-column">
                        <section className="description-card">
                            <h2>SZÁLLÁS LEÍRÁSA</h2>
                            <p>{accommodation.leiras || "Nincs elérhető leírás ehhez a szálláshoz."}</p>
                            
                            {accommodation.kerekparosBarat && (
                                <div className="cycling-extra">
                                    <p><strong>Kerékpáros barát szolgáltatások:</strong> Zárt tároló, szervizállvány és alapvető szerszámok biztosítottak.</p>
                                </div>
                            )}
                        </section>

                        <section className="amenities-section">
                            <h3>SZOLGÁLTATÁSOK</h3>
                            <div className="amenity-icons">
                                {accommodation.szallasSzolgaltatasok && accommodation.szallasSzolgaltatasok.length > 0 ? (
                                    accommodation.szallasSzolgaltatasok.map((item, index) => (
                                        <div key={index} className="amenity-item">
                                            <span className="amenity-name">
                                                {item.szolgaltatasok?.nev || "Szolgáltatás"}
                                            </span>
                                        </div>
                                    ))
                                ) : (
                                    <p>Nincsenek megadott szolgáltatások.</p>
                                )}
                            </div>
                        </section>

                        <section className="reviews-section" id="review-section">
                            <h3>VENDÉGVÉLEMÉNYEK ({reviews.length})</h3>
                            <div className="reviews-list scrollable-reviews">
                                {reviews.length > 0 ? (
                                    reviews.map((rev, idx) => {
                                        const currentScore = Number(rev.pont) || 0;
                                        return (
                                            <div key={idx} className="review-item">
                                                <div className="review-header">
                                                    <div className="review-stars">
                                                        {[...Array(5)].map((_, i) => (
                                                            <span 
                                                                key={i} 
                                                                className="material-symbols-outlined" 
                                                                style={{ color: i < currentScore ? '#d4a373' : '#e0e0e0', fontSize: '18px' }}
                                                            >
                                                                star
                                                            </span>
                                                        ))}
                                                        <span className="rating-num">{currentScore}.0</span>
                                                    </div>
                                                    <span className="review-date">
                                                        {rev.datum ? new Date(rev.datum).toLocaleDateString('hu-HU') : ''}
                                                    </span>
                                                </div>
                                                <p className="review-text">"{rev.szoveg}"</p>
                                                <div className="review-footer">
                                                    <span className="material-symbols-outlined">person</span>
                                                    <small>Vendégünk</small> 
                                                </div>
                                            </div>
                                        );
                                    })
                                ) : (
                                    <div className="no-reviews">
                                        <p>Még nem érkezett vélemény erről a szálláshelyről.</p>
                                    </div>
                                )}
                            </div>
                        </section>

                        <section className="route-map-section">
                            <h3>A TÚRA ÚTVONALA</h3>
                            <div className="route-image-wrapper">
                                <img 
                                    src={tourMapImage} 
                                    alt="Balaton Cycling Route" 
                                    className="route-image clickable-image"
                                    onClick={() => setCurrentImageIndex(allImages.length - 1)}
                                />
                            </div>
                        </section>
                    </div>

                    <aside className="booking-sidebar">
                        <div className="booking-card">
                            <div className="card-header">
                                <div className="price-box">
                                    <span className="price-big">{accommodation.ar?.toLocaleString()} Ft</span>
                                    <span className="price-small">/ éjszaka</span>
                                </div>
                                <div className="rating-pill">
                                    <span className="material-symbols-outlined">star</span>
                                    <strong>{averageRating}</strong>
                                </div>
                            </div>

                            <div className="booking-inputs">
                                <div className="input-group">
                                    <label>Érkezés napja (1 éjszaka)</label>
                                    <input 
                                        type="date" 
                                        value={bookingDate} 
                                        onChange={(e) => setBookingDate(e.target.value)} 
                                        min={new Date().toISOString().split('T')[0]} 
                                    />
                                </div>
                                
                                <div className="input-group">
                                    <label>Vendégek száma</label>
                                    <select value={guestCount} onChange={(e) => setGuestCount(e.target.value)}>
                                        <option value="1 felnőtt">1 felnőtt</option>
                                        <option value="2 felnőtt">2 felnőtt</option>
                                        <option value="2 felnőtt + 1 gyerek">2 felnőtt + 1 gyerek</option>
                                    </select>
                                </div>
                            </div>

                            {/* Pass the state to the payment page via navigate! */}
                            <button 
                                className="btn-book-now" 
                                onClick={() => navigate(`/payment/${id}`, { state: { arrivalDate: bookingDate, guests: guestCount } })}
                            >
                                Foglalás indítása
                            </button>
                            <p className="no-charge-msg">Nem vonunk le összeget a foglalás pillanatában</p>
                        </div>

                        <div className="real-map-container" style={{ marginTop: '20px', borderRadius: '15px', overflow: 'hidden', height: '250px' }}>
                            <iframe 
                                title="Accommodation Location"
                                width="100%" 
                                height="100%" 
                                style={{ border: 0 }} 
                                loading="lazy" 
                                allowFullScreen 
                                src={`https://maps.google.com/maps?q=${mapQuery}&t=&z=15&ie=UTF8&iwloc=&output=embed`}
                            ></iframe>
                        </div>
                    </aside>
                </div>
            </main>

            {/* --- IMAGE SLIDER MODAL --- */}
            {currentImageIndex !== null && (
                <div className="image-modal-overlay" onClick={() => setCurrentImageIndex(null)}>
                    
                    <button className="close-modal-btn" onClick={() => setCurrentImageIndex(null)}>✕</button>
                    
                    <button className="slider-arrow left-arrow" onClick={handlePrevImage}>❮</button>
                    
                    <div className="image-modal-content" onClick={(e) => e.stopPropagation()}>
                        <img src={allImages[currentImageIndex]} alt="Nagyított kép" />
                        
                        <div className="slider-counter">
                            {currentImageIndex + 1} / {allImages.length}
                        </div>
                    </div>

                    <button className="slider-arrow right-arrow" onClick={handleNextImage}>❯</button>

                </div>
            )}
        </div>
    );
};

export default AccommodationDetails;