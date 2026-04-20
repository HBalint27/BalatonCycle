import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import axios from 'axios';
import './Profile.css';

const VALID_CITIES = [
    "Alsóörs", "Aszófő", "Badacsonytomaj", "Badacsonytördemic", "Balatonakali", "Balatonakarattya", 
    "Balatonalmádi", "Balatonberény", "Balatonboglár", "Balatonederics", "Balatonfenyves", 
    "Balatonföldvár", "Balatonfüred", "Balatonfűzfő", "Balatongyörök", "Balatonkenese", 
    "Balatonkeresztúr", "Balatonlelle", "Balatonmáriafürdő", "Balatonőszöd", "Balatonrendes", 
    "Balatonszárszó", "Balatonszemes", "Balatonszepezd", "Balatonudvari", "Balatonvilágos", 
    "Csopak", "Fonyód", "Gyenesdiás", "Hévíz", "Keszthely", "Kővágóörs", "Örvényes", "Paloznak", 
    "Révfülöp", "Siófok", "Szántód", "Szigliget", "Tihany", "Vonyarcvashegy", "Zamárdi", "Zánka"
];

const ProfilePage = () => {
    const { t } = useTranslation();
    const location = useLocation();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState('adatok');
    
    const [userData, setUserData] = useState({
        fid: 0, nev: '', email: '', lakcim: '', telefonszam: '', statusz: ''
    });

    const [myAccommodations, setMyAccommodations] = useState([]);
    const [adsLoading, setAdsLoading] = useState(false);

    // States for Inline Editing
    const [editingAdId, setEditingAdId] = useState(null);
    const [editFormData, setEditFormData] = useState({});
    const [editFormErrors, setEditFormErrors] = useState({});
    const [isSavingEdit, setIsSavingEdit] = useState(false);

    const [loading, setLoading] = useState(true);
    const [statusMsg, setStatusMsg] = useState({ type: '', text: '' });

    // States for Bookings & Reviews
    const [myBookings, setMyBookings] = useState([]);
    const [bookingsLoading, setBookingsLoading] = useState(false);
    
    const [reviewingSzid, setReviewingSzid] = useState(null);
    const [reviewData, setReviewData] = useState({ pont: 5, szoveg: '' });
    const [isSubmittingReview, setIsSubmittingReview] = useState(false);

    // NEW: Keep track of which bookings we've already reviewed
    const [reviewedIds, setReviewedIds] = useState(() => {
        const saved = localStorage.getItem('reviewedIds');
        return saved ? JSON.parse(saved) : [];
    });

    const API_BASE = "https://localhost:7284/api";

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const tab = params.get('tab');
        if (tab) setActiveTab(tab);
        fetchUserData();
    }, [location]);

    useEffect(() => {
        if (activeTab === 'hirdeteseim') {
            fetchMyAds();
        } else if (activeTab === 'foglalasok') {
            fetchMyBookings();
        }
    }, [activeTab]);

    const fetchUserData = async () => {
        try {
            const token = localStorage.getItem('token');
            if (!token) { navigate('/login'); return; }
            const response = await axios.get(`${API_BASE}/login/me`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setUserData(response.data);
        } catch (error) {
            if (error.response?.status === 401) {
                localStorage.removeItem('token');
                navigate('/login');
            }
            setStatusMsg({ type: 'error', text: t('profile.fetch_error') });
        } finally {
            setLoading(false);
        }
    };

    const fetchMyAds = async () => {
        setAdsLoading(true);
        try {
            const token = localStorage.getItem('token');
            const response = await axios.get(`${API_BASE}/Szallas/my-accommodations`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setMyAccommodations(response.data);
        } catch (error) {
            console.error("Hiba a hirdetések lekérésekor:", error);
        } finally {
            setAdsLoading(false);
        }
    };

    // --- NEW: Fetch Bookings ---
    const fetchMyBookings = async () => {
        setBookingsLoading(true);
        try {
            const token = localStorage.getItem('token');
            const response = await axios.get(`${API_BASE}/Szallas/my-bookings`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setMyBookings(response.data);
        } catch (error) {
            console.error("Hiba a foglalások lekérésekor:", error);
        } finally {
            setBookingsLoading(false);
        }
    };

    const handleDeleteAd = async (szid) => {
        if (window.confirm("Biztosan törölni szeretnéd ezt a hirdetést? Ezt nem lehet visszavonni.")) {
            try {
                const token = localStorage.getItem('token');
                await axios.delete(`${API_BASE}/Szallas/${szid}`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                setMyAccommodations(prevAds => prevAds.filter(ad => ad.szid !== szid));
                alert("Hirdetés sikeresen törölve!");
                if (editingAdId === szid) setEditingAdId(null);
            } catch (error) {
                if (error.response?.status === 403 || error.response?.status === 401) {
                    alert("Nincs jogosultságod törölni ezt a hirdetést.");
                } else {
                    alert("Hiba történt a törlés során.");
                }
            }
        }
    };

    const toggleEditForm = (ad) => {
        if (editingAdId === ad.szid) {
            setEditingAdId(null);
        } else {
            setEditingAdId(ad.szid);
            setEditFormData({
                nev: ad.nev || '',
                iranyitoszam: ad.iranyitoszam || '',
                telepules: ad.telepules || '',
                utca: ad.utca || '',
                hazszam: ad.hazszam || '',
                ar: ad.ar || '',
                leiras: ad.leiras || ''
            });
            setEditFormErrors({});
        }
    };

    const handleEditChange = (e) => {
        const { name, value } = e.target;
        if (editFormErrors[name]) setEditFormErrors(prev => ({ ...prev, [name]: null }));
        setEditFormData(prev => ({ ...prev, [name]: value }));
    };

    const validateEditForm = () => {
        const errors = {};
        if (editFormData.nev.trim().length < 5) errors.nev = "Minimum 5 karakter.";
        if (!/^[1-9][0-9]{3}$/.test(editFormData.iranyitoszam)) errors.iranyitoszam = "Érvénytelen (4 számjegy).";
        if (!VALID_CITIES.some(city => city.toLowerCase() === editFormData.telepules.toLowerCase().trim())) errors.telepules = "Érvénytelen Balatoni település.";
        if (editFormData.utca.trim().length < 2) errors.utca = "Utca megadása kötelező.";
        if (String(editFormData.hazszam).trim().length < 1) errors.hazszam = "Házszám kötelező.";
        if (!editFormData.ar || Number(editFormData.ar) < 2000) errors.ar = "Min. 2000 Ft.";
        if (editFormData.leiras.trim().length < 30) errors.leiras = "Túl rövid leírás.";
        return errors;
    };

    const submitEdit = async (e, szid) => {
        e.preventDefault();
        const errors = validateEditForm();
        if (Object.keys(errors).length > 0) {
            setEditFormErrors(errors);
            return;
        }

        setIsSavingEdit(true);
        try {
            const submitData = new FormData();
            submitData.append('Nev', editFormData.nev);
            submitData.append('Iranyitoszam', editFormData.iranyitoszam);
            submitData.append('Telepules', editFormData.telepules.trim());
            submitData.append('Utca', editFormData.utca);
            submitData.append('Hazszam', editFormData.hazszam);
            submitData.append('Ar', editFormData.ar);
            submitData.append('Leiras', editFormData.leiras);
            submitData.append('Lat', 46.8);
            submitData.append('Lon', 17.5);

            const token = localStorage.getItem('token');
            await axios.put(`${API_BASE}/Szallas/${szid}`, submitData, {
                headers: { 
                    'Authorization': `Bearer ${token}`
                }
            });

            setMyAccommodations(prev => prev.map(ad => ad.szid === szid ? { ...ad, ...editFormData } : ad));
            setEditingAdId(null); 
            alert("Sikeresen módosítva!");
        } catch (error) {
            console.error("Hiba a mentés során:", error);
            alert("Hiba történt a módosítás során.");
        } finally {
            setIsSavingEdit(false);
        }
    };

    // --- NEW: Submit Review ---
    const submitReview = async (e, szid) => {
        e.preventDefault();
        setIsSubmittingReview(true);
        try {
            const token = localStorage.getItem('token');
            await axios.post(`${API_BASE}/Szallas/${szid}/review`, reviewData, {
                headers: { Authorization: `Bearer ${token}` }
            });
            alert("Köszönjük az értékelést!");
            
            // --- NEW: Mark as reviewed and save to LocalStorage ---
            const updatedReviewedIds = [...reviewedIds, szid];
            setReviewedIds(updatedReviewedIds);
            localStorage.setItem('reviewedIds', JSON.stringify(updatedReviewedIds));

            setReviewingSzid(null);
            setReviewData({ pont: 5, szoveg: '' });
        } catch (error) {
            alert(error.response?.data?.message || "Hiba történt az értékelés küldésekor.");
        } finally {
            setIsSubmittingReview(false);
        }
    };

    const handleUpdate = async (e) => {
        e.preventDefault();
        setStatusMsg({ type: '', text: '' });
        try {
            const token = localStorage.getItem('token');
            await axios.put(`${API_BASE}/Felhasznalo/${userData.fid}`, userData, {
                headers: { 
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json"
                }
            });
            setStatusMsg({ type: 'success', text: t('profile.save_success') });
        } catch (error) {
            setStatusMsg({ type: 'error', text: t('profile.save_error') });
        }
    };

    const handleDeleteProfile = async () => {
        if (window.confirm(t('profile.delete_confirm'))) {
            try {
                const token = localStorage.getItem('token');
                await axios.delete(`${API_BASE}/Felhasznalo/${userData.fid}`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                alert(t('profile.delete_success'));
                localStorage.removeItem('token');
                navigate('/');
                window.location.reload(); 
            } catch (error) {
                setStatusMsg({ type: 'error', text: t('profile.delete_error') });
            }
        }
    };

    const renderContent = () => {
        if (loading) return <div className="loader">{t('profile.loading')}</div>;

        switch (activeTab) {
            case 'adatok':
                return (
                    <div className="tab-content">
                        <h2>👤 {t('profile.tabs.data_title')}</h2>
                        {statusMsg.text && (
                            <div className={`alert ${statusMsg.type}`}>{statusMsg.text}</div>
                        )}
                        <form className="profile-form" onSubmit={handleUpdate}>
                            <div className="form-row">
                                <label>{t('profile.form.name')}</label>
                                <input 
                                    type="text" 
                                    value={userData.nev || ''} 
                                    onChange={(e) => setUserData({...userData, nev: e.target.value})} 
                                />
                            </div>
                            <div className="form-row">
                                <label>{t('profile.form.email')}</label>
                                <input type="email" value={userData.email || ''} readOnly className="disabled-input" title={t('profile.form.email_readonly')} />
                            </div>
                            <div className="form-row">
                                <label>{t('profile.form.address')}</label>
                                <input 
                                    type="text" 
                                    value={userData.lakcim || ''} 
                                    onChange={(e) => setUserData({...userData, lakcim: e.target.value})} 
                                />
                            </div>
                            <div className="form-row">
                                <label>{t('profile.form.phone')}</label>
                                <input 
                                    type="text" 
                                    value={userData.telefonszam || ''} 
                                    onChange={(e) => setUserData({...userData, telefonszam: e.target.value})} 
                                />
                            </div>
                            <button type="submit" className="btn-save">{t('profile.form.btn_save')}</button>
                            <button type="button" className="btn-delete" onClick={handleDeleteProfile}>{t('profile.form.btn_delete')}</button>
                        </form>
                    </div>
                );
            case 'hirdeteseim':
                return (
                    <div className="tab-content">
                        <h2>🏠 {t('profile.tabs.ads_title')}</h2>
                        {adsLoading ? (
                            <div className="loader">Hirdetések betöltése...</div>
                        ) : myAccommodations.length > 0 ? (
                            <div className="user-ads-list">
                                {myAccommodations.map(ad => (
                                    <div key={ad.szid} className="ad-card-wrapper">
                                        <div className="user-ad-card" onClick={() => navigate(`/accommodation/${ad.szid}`)}>
                                            <img src={`https://localhost:7284/${ad.szallaskep}`} alt={ad.nev} />
                                            <div className="ad-info">
                                                <h4>{ad.nev}</h4>
                                                <p>{ad.telepules}, {ad.utca} {ad.hazszam}</p>
                                                <span className="ad-price">{ad.ar?.toLocaleString()} Ft / éjszaka</span>
                                            </div>
                                            <div className="ad-actions" onClick={(e) => e.stopPropagation()}>
                                                <button className={`btn-icon-edit ${editingAdId === ad.szid ? 'active-edit' : ''}`} onClick={() => toggleEditForm(ad)}>✏️</button>
                                                <button className="btn-icon-delete" onClick={() => handleDeleteAd(ad.szid)}>🗑️</button>
                                            </div>
                                        </div>

                                        <div className={`inline-edit-form ${editingAdId === ad.szid ? 'show' : ''}`}>
                                            <form onSubmit={(e) => submitEdit(e, ad.szid)}>
                                                <div className="edit-grid">
                                                    <div className="edit-group full-span">
                                                        <label>Szállás Neve</label>
                                                        <input type="text" name="nev" value={editFormData.nev || ''} onChange={handleEditChange} className={editFormErrors.nev ? 'input-error' : ''}/>
                                                        {editFormErrors.nev && <span className="error-text">{editFormErrors.nev}</span>}
                                                    </div>
                                                    
                                                    <div className="edit-group">
                                                        <label>Település</label>
                                                        <input type="text" name="telepules" value={editFormData.telepules || ''} onChange={handleEditChange} list="edit-city-list" className={editFormErrors.telepules ? 'input-error' : ''}/>
                                                        <datalist id="edit-city-list">
                                                            {VALID_CITIES.map((city, idx) => <option key={idx} value={city} />)}
                                                        </datalist>
                                                        {editFormErrors.telepules && <span className="error-text">{editFormErrors.telepules}</span>}
                                                    </div>

                                                    <div className="edit-group">
                                                        <label>Irányítószám</label>
                                                        <input type="number" name="iranyitoszam" value={editFormData.iranyitoszam || ''} onChange={handleEditChange} className={editFormErrors.iranyitoszam ? 'input-error' : ''}/>
                                                        {editFormErrors.iranyitoszam && <span className="error-text">{editFormErrors.iranyitoszam}</span>}
                                                    </div>

                                                    <div className="edit-group">
                                                        <label>Utca</label>
                                                        <input type="text" name="utca" value={editFormData.utca || ''} onChange={handleEditChange} className={editFormErrors.utca ? 'input-error' : ''}/>
                                                        {editFormErrors.utca && <span className="error-text">{editFormErrors.utca}</span>}
                                                    </div>

                                                    <div className="edit-group">
                                                        <label>Házszám</label>
                                                        <input type="text" name="hazszam" value={editFormData.hazszam || ''} onChange={handleEditChange} className={editFormErrors.hazszam ? 'input-error' : ''}/>
                                                        {editFormErrors.hazszam && <span className="error-text">{editFormErrors.hazszam}</span>}
                                                    </div>

                                                    <div className="edit-group">
                                                        <label>Ár / éjszaka (Ft)</label>
                                                        <input type="number" name="ar" value={editFormData.ar || ''} onChange={handleEditChange} className={editFormErrors.ar ? 'input-error' : ''}/>
                                                        {editFormErrors.ar && <span className="error-text">{editFormErrors.ar}</span>}
                                                    </div>

                                                    <div className="edit-group full-span">
                                                        <label>Leírás</label>
                                                        <textarea name="leiras" value={editFormData.leiras || ''} onChange={handleEditChange} rows="3" className={editFormErrors.leiras ? 'input-error' : ''}></textarea>
                                                        {editFormErrors.leiras && <span className="error-text">{editFormErrors.leiras}</span>}
                                                    </div>
                                                </div>
                                                <div className="edit-form-actions">
                                                    <button type="button" className="btn-cancel" onClick={() => setEditingAdId(null)}>Mégse</button>
                                                    <button type="submit" className="btn-save" disabled={isSavingEdit}>{isSavingEdit ? 'Mentés...' : 'Változtatások Mentése'}</button>
                                                </div>
                                            </form>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <p className="empty-msg">{t('profile.tabs.empty_ads')}</p>
                        )}
                    </div>
                );
            case 'foglalasok':
                return (
                    <div className="tab-content">
                        <h2>📅 {t('profile.tabs.bookings_title')}</h2>
                        {bookingsLoading ? (
                            <div className="loader">Foglalások betöltése...</div>
                        ) : myBookings.length > 0 ? (
                            <div className="user-ads-list">
                                {myBookings.map((booking, idx) => {
                                    // Set hours to 0 to compare purely by date
                                    const today = new Date();
                                    today.setHours(0, 0, 0, 0);
                                    const bookingDate = new Date(booking.erkezesNap);
                                    bookingDate.setHours(0, 0, 0, 0);
                                    
                                    const isPast = bookingDate <= today;
                                    
                                    return (
                                        <div key={idx} className="ad-card-wrapper">
                                            <div className="user-ad-card" onClick={() => navigate(`/accommodation/${booking.szid}`)}>
                                                <img src={booking.szallaskep ? `https://localhost:7284/${booking.szallaskep}` : 'https://via.placeholder.com/150'} alt={booking.nev} />
                                                <div className="ad-info">
                                                    <h4>{booking.nev}</h4>
                                                    <p>{booking.telepules}</p>
                                                    <span className="ad-price">
                                                        Érkezés: {new Date(booking.erkezesNap).toLocaleDateString('hu-HU')}
                                                    </span>
                                                </div>
                                                <div className="ad-actions" onClick={(e) => e.stopPropagation()}>
                                                    {isPast ? (
                                                        reviewedIds.includes(booking.szid) ? (
                                                            /* --- GRAYED OUT STAR --- */
                                                            <button 
                                                                className="btn-icon-edit disabled-star" 
                                                                title="Már értékelted ezt a szállást"
                                                                disabled
                                                            >
                                                                ⭐
                                                            </button>
                                                        ) : (
                                                            /* --- ACTIVE STAR --- */
                                                            <button 
                                                                className={`btn-icon-edit ${reviewingSzid === booking.szid ? 'active-edit' : ''}`} 
                                                                onClick={() => setReviewingSzid(reviewingSzid === booking.szid ? null : booking.szid)}
                                                                title="Értékelés írása"
                                                            >
                                                                ⭐
                                                            </button>
                                                        )
                                                    ) : (
                                                        <span className="future-badge">Hamarosan</span>
                                                    )}
                                                </div>
                                            </div>

                                            {/* --- INLINE REVIEW FORM --- */}
                                            <div className={`inline-edit-form ${reviewingSzid === booking.szid ? 'show' : ''}`}>
                                                <form onSubmit={(e) => submitReview(e, booking.szid)}>
                                                    <div className="edit-grid" style={{ gridTemplateColumns: '1fr' }}>
                                                        <div className="edit-group">
                                                            <label>Értékelés (1-5 csillag)</label>
                                                            <input 
                                                                type="range" 
                                                                min="1" max="5" 
                                                                value={reviewData.pont} 
                                                                onChange={(e) => setReviewData({...reviewData, pont: parseInt(e.target.value)})} 
                                                            />
                                                            <div style={{ textAlign: 'center', fontSize: '1.8rem', color: '#d9aa78', marginTop: '10px' }}>
                                                                {'★'.repeat(reviewData.pont)}{'☆'.repeat(5 - reviewData.pont)}
                                                            </div>
                                                        </div>
                                                        <div className="edit-group">
                                                            <label>Szöveges vélemény</label>
                                                            <textarea 
                                                                rows="3" 
                                                                required
                                                                placeholder="Milyen volt a szállás? Ossza meg tapasztalatait..."
                                                                value={reviewData.szoveg}
                                                                onChange={(e) => setReviewData({...reviewData, szoveg: e.target.value})}
                                                            ></textarea>
                                                        </div>
                                                    </div>
                                                    <div className="edit-form-actions">
                                                        <button type="button" className="btn-cancel" onClick={() => setReviewingSzid(null)}>Mégse</button>
                                                        <button type="submit" className="btn-save" disabled={isSubmittingReview}>
                                                            {isSubmittingReview ? 'Küldés...' : 'Értékelés Küldése'}
                                                        </button>
                                                    </div>
                                                </form>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        ) : (
                            <p className="empty-msg">{t('profile.tabs.empty_bookings')}</p>
                        )}
                    </div>
                );
            default: return null;
        }
    };

    return (
        <div className="profile-wrapper">
            <div className="profile-container">
                <aside className="profile-sidebar">
                    <div className="sidebar-header"><h3>{t('profile.sidebar_title')}</h3></div>
                    <button className={activeTab === 'adatok' ? 'active' : ''} onClick={() => setActiveTab('adatok')}>{t('profile.tabs.data_title')}</button>
                    <button className={activeTab === 'hirdeteseim' ? 'active' : ''} onClick={() => setActiveTab('hirdeteseim')}>{t('profile.tabs.ads_title')}</button>
                    <button className={activeTab === 'foglalasok' ? 'active' : ''} onClick={() => setActiveTab('foglalasok')}>{t('profile.tabs.bookings_title')}</button>
                </aside>
                <main className="profile-main">{renderContent()}</main>
            </div>
            <div className="profile-footer">
                <button className="btn-back-home" onClick={() => navigate('/')}>← {t('profile.back_home')}</button>
            </div>
        </div>
    );
};

export default ProfilePage;