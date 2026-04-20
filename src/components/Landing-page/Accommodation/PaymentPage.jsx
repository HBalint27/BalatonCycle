import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import axios from 'axios';
import './PaymentPage.css';

const PaymentPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const location = useLocation();
    
    // Grab the data passed from the Details Page (or use fallbacks if they refresh the page)
    const { arrivalDate, guests } = location.state || { 
        arrivalDate: new Date().toISOString().split('T')[0], 
        guests: "2 felnőtt" 
    };

    const [accommodation, setAccommodation] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isProcessing, setIsProcessing] = useState(false);

    // Credit Card UX States
    const [cardNumber, setCardNumber] = useState('');
    const [expiry, setExpiry] = useState('');
    const [cvv, setCvv] = useState('');

    const API_BASE = "https://localhost:7284/api";

    useEffect(() => {
        axios.get(`${API_BASE}/szallas/${id}?ext=true`)
            .then(res => {
                setAccommodation(res.data);
                setLoading(false);
            })
            .catch(err => {
                console.error("Hiba a lekérdezéskor:", err);
                setLoading(false);
            });
    }, [id]);

    // --- UX: Auto-formatting Credit Card Inputs ---
    const handleCardNumber = (e) => {
        let val = e.target.value.replace(/\D/g, ''); // Remove all non-digits
        val = val.replace(/(.{4})/g, '$1 ').trim(); // Add space every 4 chars
        setCardNumber(val);
    };

    const handleExpiry = (e) => {
        let val = e.target.value.replace(/\D/g, ''); // Remove all non-digits
        if (val.length >= 2) {
            val = val.substring(0, 2) + '/' + val.substring(2, 4); // Add slash
        }
        setExpiry(val);
    };

    const handleCvv = (e) => {
        let val = e.target.value.replace(/\D/g, ''); // Remove all non-digits
        setCvv(val);
    };

    const handlePaymentSubmit = async (e) => {
        e.preventDefault();
        setIsProcessing(true);

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                alert("Kérjük, jelentkezzen be a foglaláshoz!");
                navigate('/login');
                return;
            }

            await axios.post(`${API_BASE}/szallas/${id}/book`, 
                { ErkezesNap: arrivalDate },
                { headers: { Authorization: `Bearer ${token}` }}
            );

            setTimeout(() => {
                alert("Sikeres fizetés és foglalás!");
                navigate('/profile?tab=foglalasok');
            }, 1500);

        } catch (error) {
            alert(error.response?.data?.message || "Hiba történt a foglalás során.");
            setIsProcessing(false);
        }
    };

    if (loading) return <div className="loader">Betöltés...</div>;
    if (!accommodation) return <div className="error">Szállás nem található.</div>;

    const mainImageUrl = accommodation.szallaskep 
        ? `https://localhost:7284/${accommodation.szallaskep}` 
        : 'https://via.placeholder.com/400x300?text=Nincs+kép';

    const price = accommodation.ar || 0;
    const serviceFee = 2500;
    const tax = Math.round(price * 0.05);
    const total = price + serviceFee + tax;

    return (
        <div className="payment-page-wrapper">
            <main className="payment-container">
                <div className="payment-header">
                    <h1 className="payment-title">Biztonságos Fizetés</h1>
                    <p className="payment-subtitle">Véglegesítse utazását a Balaton partjára.</p>
                </div>

                <form className="payment-grid" onSubmit={handlePaymentSubmit}>
                    
                    {/* LEFT COLUMN */}
                    <div className="payment-left-col">
                        
                        <section className="pay-card summary-section">
                            <h2>Foglalás Összegzése</h2>
                            <div className="summary-content">
                                <img src={mainImageUrl} alt="Szállás" className="summary-img" />
                                <div className="summary-info">
                                    <h3>{accommodation.nev}</h3>
                                    <div className="summary-meta">
                                        <span>📍 {accommodation.telepules}, {accommodation.utca}</span>
                                        <span className="text-primary">✔️ Visszaigazolt Ár</span>
                                    </div>
                                </div>
                            </div>
                        </section>

                        {/* CHANGED: Static Info Reassurance */}
                        <section className="pay-card guest-section">
                            <h2>Foglalás Adatai</h2>
                            <div className="static-info-grid">
                                <div className="static-info-item">
                                    <span className="material-symbols-outlined text-primary">calendar_month</span>
                                    <div>
                                        <small>Érkezés Napja (1 éjszaka)</small>
                                        <p>{arrivalDate}</p>
                                    </div>
                                </div>
                                <div className="static-info-item">
                                    <span className="material-symbols-outlined text-primary">group</span>
                                    <div>
                                        <small>Vendégek száma</small>
                                        <p>{guests}</p>
                                    </div>
                                </div>
                            </div>
                        </section>

                        {/* CHANGED: Dynamic Real Services */}
                        <section className="pay-card payment-amenities-section">
                            <h2>Garantált Szolgáltatások</h2>
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
                    </div>

                    {/* RIGHT COLUMN */}
                    <aside className="payment-right-col">
                        <div className="pay-card breakdown-section">
                            <h2>Ár Részletezése</h2>
                            <div className="breakdown-lines">
                                <div className="breakdown-line">
                                    <span>Szállás díja (1 éjszaka)</span>
                                    <span>{price.toLocaleString()} Ft</span>
                                </div>
                                <div className="breakdown-line">
                                    <span>Kezelési költség</span>
                                    <span>{serviceFee.toLocaleString()} Ft</span>
                                </div>
                                <div className="breakdown-line">
                                    <span>IFA (Idegenforgalmi adó)</span>
                                    <span>{tax.toLocaleString()} Ft</span>
                                </div>
                            </div>
                            <div className="breakdown-total">
                                <span>Összesen (HUF)</span>
                                <span className="total-price">{total.toLocaleString()} Ft</span>
                            </div>
                        </div>

                        <div className="pay-card method-section">
                            <h2>Fizetési Mód</h2>
                            <div className="payment-methods">
                                <label className="method-label active">
                                    <input type="radio" name="paymethod" defaultChecked />
                                    <span className="method-box">💳 Bankkártya</span>
                                </label>
                                <label className="method-label disabled" title="Jelenleg nem elérhető">
                                    <input type="radio" name="paymethod" disabled />
                                    <span className="method-box">🅿️ PayPal</span>
                                </label>
                            </div>

                            {/* CHANGED: Restricted & Formatting Inputs */}
                            <div className="credit-card-form">
                                <div className="form-group">
                                    <label>Kártyaszám</label>
                                    <input 
                                        type="text" 
                                        placeholder="0000 0000 0000 0000" 
                                        maxLength="19" 
                                        value={cardNumber}
                                        onChange={handleCardNumber}
                                        required 
                                    />
                                </div>
                                <div className="form-group-row">
                                    <div className="form-group">
                                        <label>Lejárat (HH/ÉÉ)</label>
                                        <input 
                                            type="text" 
                                            placeholder="MM/YY" 
                                            maxLength="5" 
                                            value={expiry}
                                            onChange={handleExpiry}
                                            required 
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>CVV</label>
                                        <input 
                                            type="text" 
                                            placeholder="123" 
                                            maxLength="3" 
                                            value={cvv}
                                            onChange={handleCvv}
                                            required 
                                        />
                                    </div>
                                </div>
                            </div>

                            <button type="submit" className="btn-book-final" disabled={isProcessing}>
                                {isProcessing ? 'Feldolgozás...' : 'Fizetés és Foglalás'}
                            </button>
                            <p className="secure-badge">🔒 Titkosított és biztonságos tranzakció</p>
                        </div>
                    </aside>
                </form>
            </main>
        </div>
    );
};

export default PaymentPage;