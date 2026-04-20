import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './AddAccommodation.css';

const CITY_DATA = {
    "Alsóörs": "8226", "Aszófő": "8241", "Badacsonytomaj": "8258", "Badacsonytördemic": "8263", 
    "Balatonakali": "8243", "Balatonakarattya": "8172", "Balatonalmádi": "8220", "Balatonberény": "8649", 
    "Balatonboglár": "8630", "Balatonederics": "8312", "Balatonfenyves": "8646", "Balatonföldvár": "8623", 
    "Balatonfüred": "8230", "Balatonfűzfő": "8175", "Balatongyörök": "8313", "Balatonkenese": "8174", 
    "Balatonkeresztúr": "8648", "Balatonlelle": "8638", "Balatonmáriafürdő": "8647", "Balatonőszöd": "8637", 
    "Balatonrendes": "8255", "Balatonszárszó": "8624", "Balatonszemes": "8636", "Balatonszepezd": "8252", 
    "Balatonudvari": "8242", "Balatonvilágos": "8171", "Csopak": "8229", "Fonyód": "8640", 
    "Gyenesdiás": "8315", "Hévíz": "8380", "Keszthely": "8360", "Kővágóörs": "8254", 
    "Örvényes": "8242", "Paloznak": "8229", "Révfülöp": "8253", "Siófok": "8600", 
    "Szántód": "8622", "Szigliget": "8264", "Tihany": "8237", "Vonyarcvashegy": "8314", 
    "Zamárdi": "8621", "Zánka": "8251"
};
const VALID_CITIES = Object.keys(CITY_DATA);

const AddAccommodation = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    
    const [amenities, setAmenities] = useState([]);
    const [selectedAmenities, setSelectedAmenities] = useState([]);

    const [mainImagePreview, setMainImagePreview] = useState(null);
    const [mainImageFile, setMainImageFile] = useState(null);
    const [galleryPreviews, setGalleryPreviews] = useState([]);
    const [galleryFiles, setGalleryFiles] = useState([]);
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    const [formErrors, setFormErrors] = useState({});

    // REMOVED tid: 1 from here! The backend handles it securely now.
    const [formData, setFormData] = useState({
        nev: '', iranyitoszam: '', telepules: '', utca: '', hazszam: '',
        ar: '', leiras: '', lat: 46.8, lon: 17.5
    });

    useEffect(() => {
        fetch('https://localhost:7284/api/szolgaltatasok')
            .then(res => res.json())
            .then(data => setAmenities(data))
            .catch(err => console.error("Hiba a szolgáltatások betöltésekor:", err));
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        
        if (formErrors[name]) {
            setFormErrors(prev => ({ ...prev, [name]: null }));
        }

        if (name === 'telepules') {
            const matchedCity = VALID_CITIES.find(c => c.toLowerCase() === value.toLowerCase());
            if (matchedCity) {
                setFormData(prev => ({ ...prev, telepules: matchedCity, iranyitoszam: CITY_DATA[matchedCity] }));
                setFormErrors(prev => ({ ...prev, telepules: null, iranyitoszam: null }));
                return;
            }
        }
        setFormData({ ...formData, [name]: value });
    };

    const handleAmenityChange = (id) => {
        setSelectedAmenities(prev => 
            prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
        );
    };

    const handleMainImageChange = (e) => {
        if (e.target.files && e.target.files.length > 0) {
            setMainImageFile(e.target.files[0]);
            setMainImagePreview(URL.createObjectURL(e.target.files[0]));
            if (formErrors.kepadat) setFormErrors(prev => ({ ...prev, kepadat: null }));
        }
    };

    const handleGalleryChange = (e) => {
        if (e.target.files) {
            const files = Array.from(e.target.files);
            setGalleryFiles(prev => [...prev, ...files]);
            const previews = files.map(file => URL.createObjectURL(file));
            setGalleryPreviews(prev => [...prev, ...previews]);
        }
    };

    const validateForm = () => {
        const errors = {};
        
        if (formData.nev.trim().length < 5) errors.nev = "A szállás neve legalább 5 karakter kell legyen.";
        
        if (!/^[1-9][0-9]{3}$/.test(formData.iranyitoszam)) errors.iranyitoszam = "Érvénytelen irányítószám (4 számjegy).";
        
        const isValidCity = VALID_CITIES.some(city => city.toLowerCase() === formData.telepules.toLowerCase().trim());
        if (!isValidCity) errors.telepules = "Kérjük, válasszon a listában szereplő Balaton-parti települések közül.";
        
        if (formData.utca.trim().length < 2) errors.utca = "Kérjük, adja meg az utca nevét.";
        if (formData.hazszam.trim().length < 1) errors.hazszam = "Kérjük, adja meg a házszámot.";
        
        if (!formData.ar || Number(formData.ar) < 2000) errors.ar = "Az árnak legalább 2 000 Ft-nak kell lennie.";
        
        if (formData.leiras.trim().length < 30) errors.leiras = "A leírás túl rövid. Kérjük, írjon legalább 30 karaktert.";
        
        if (!mainImageFile) errors.kepadat = "Főkép feltöltése kötelező a hirdetéshez.";

        return errors;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        const errors = validateForm();
        if (Object.keys(errors).length > 0) {
            setFormErrors(errors); 
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return; 
        }

        setIsSubmitting(true);

        try {
            const submitData = new FormData();
            submitData.append('Nev', formData.nev);
            submitData.append('Iranyitoszam', formData.iranyitoszam);
            submitData.append('Telepules', formData.telepules.trim());
            submitData.append('Utca', formData.utca);
            submitData.append('Hazszam', formData.hazszam);
            submitData.append('Ar', formData.ar);
            submitData.append('Leiras', formData.leiras);
            
            // REMOVED appending Tid here. 

            submitData.append('Lat', formData.lat);
            submitData.append('Lon', formData.lon);

            if (mainImageFile) submitData.append('kepadat', mainImageFile);
            galleryFiles.forEach(file => submitData.append('galeriaKepek', file));
            selectedAmenities.forEach(id => submitData.append('SzolgaltatasIds', id));

            const token = localStorage.getItem('token'); 
            const response = await fetch('https://localhost:7284/api/szallas', {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: submitData
            });

            if (response.ok) {
                alert("Hirdetés és galéria sikeresen közzétéve!");
                navigate('/'); 
            } else {
                const errorText = await response.text();
                if (response.status === 401) {
                    // NEW: Better handling for expired tokens
                    alert("A munkamenet lejárt. Kérjük, jelentkezzen be újra a hirdetés feladásához!");
                    localStorage.removeItem('token');
                    navigate('/login');
                } else {
                    alert("Hiba történt a mentés során: " + errorText);
                }
            }
        } catch (error) {
            alert("Hálózati hiba történt.");
        } finally {
            setIsSubmitting(false);
        }
    };

    const getInputClass = (fieldName) => `form-control ${formErrors[fieldName] ? 'input-error' : ''}`;

    return (
        <div className="add-acc-wrapper">
            <div className="add-acc-container">
                <header className="add-acc-header">
                    <h2>Hirdetés feladása</h2>
                    <p>Oszd meg kerékpárosbarát szálláshelyedet a közösséggel.</p>
                </header>

                <form className="add-acc-form" onSubmit={handleSubmit} noValidate>
                    
                    <div className="form-group full-width">
                        <label>Szállás neve</label>
                        <input type="text" name="nev" className={getInputClass('nev')} value={formData.nev} placeholder="Pl. Balatoni Bringa Panzió..." onChange={handleChange} />
                        {formErrors.nev && <span className="error-message">{formErrors.nev}</span>}
                    </div>

                    <div className="form-row">
                        <div className="form-group" style={{ flex: '0.3' }}>
                            <label>Irányítószám</label>
                            <input type="number" name="iranyitoszam" className={getInputClass('iranyitoszam')} value={formData.iranyitoszam} placeholder="8230" onChange={handleChange} />
                            {formErrors.iranyitoszam && <span className="error-message">{formErrors.iranyitoszam}</span>}
                        </div>
                        <div className="form-group" style={{ flex: '0.7' }}>
                            <label>Település</label>
                            <input type="text" name="telepules" className={getInputClass('telepules')} value={formData.telepules} list="city-list" placeholder="Válasszon települést..." onChange={handleChange} />
                            <datalist id="city-list">
                                {VALID_CITIES.map((city, index) => <option key={index} value={city} />)}
                            </datalist>
                            {formErrors.telepules && <span className="error-message">{formErrors.telepules}</span>}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group" style={{ flex: '0.7' }}>
                            <label>Utca</label>
                            <input type="text" name="utca" className={getInputClass('utca')} value={formData.utca} placeholder="Part utca" onChange={handleChange} />
                            {formErrors.utca && <span className="error-message">{formErrors.utca}</span>}
                        </div>
                        <div className="form-group" style={{ flex: '0.3' }}>
                            <label>Házszám</label>
                            <input type="text" name="hazszam" className={getInputClass('hazszam')} value={formData.hazszam} placeholder="2/A" onChange={handleChange} />
                            {formErrors.hazszam && <span className="error-message">{formErrors.hazszam}</span>}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Ár / éjszaka (Ft)</label>
                            <input type="number" name="ar" className={getInputClass('ar')} value={formData.ar} placeholder="15000" onChange={handleChange} />
                            {formErrors.ar && <span className="error-message">{formErrors.ar}</span>}
                        </div>
                    </div>

                    <div className="form-group full-width">
                        <label>Leírás</label>
                        <textarea name="leiras" className={getInputClass('leiras')} value={formData.leiras} rows="4" placeholder="Írd le, miért ideális a szállásod bicikliseknek..." onChange={handleChange}></textarea>
                        {formErrors.leiras && <span className="error-message">{formErrors.leiras}</span>}
                    </div>

                    <div className="form-group full-width">
                        <label>Szolgáltatások (Több is választható)</label>
                        <div className="amenities-grid">
                            {amenities.map(amenity => (
                                <label key={amenity.szoid} className="custom-check-item">
                                    <input 
                                        type="checkbox" 
                                        checked={selectedAmenities.includes(amenity.szoid)} 
                                        onChange={() => handleAmenityChange(amenity.szoid)} 
                                    />
                                    <span className="custom-checkmark"></span>
                                    <span className="label-text">{amenity.nev}</span>
                                </label>
                            ))}
                        </div>
                    </div>

                    <div className="image-upload-section">
                        <p className={`section-title ${formErrors.kepadat ? 'text-error' : ''}`}>Főkép feltöltése (1 kép)</p>
                        <div className={`upload-dropzone ${formErrors.kepadat ? 'dropzone-error' : ''}`}>
                            <input type="file" onChange={handleMainImageChange} id="main-file-input" className="hidden-input" accept="image/png, image/jpeg" />
                            <label htmlFor="main-file-input" className="dropzone-content">
                                <span className="upload-icon">📸</span>
                                <p className="main-text">Kattintson ide a főkép kiválasztásához</p>
                            </label>
                        </div>
                        {formErrors.kepadat && <span className="error-message" style={{marginTop: '8px'}}>{formErrors.kepadat}</span>}
                        
                        {mainImagePreview && (
                            <div className="image-preview-gallery">
                                <div className="preview-item" style={{ backgroundImage: `url(${mainImagePreview})` }}></div>
                            </div>
                        )}
                    </div>

                    <div className="image-upload-section">
                        <p className="section-title">Galéria képek (Több kép kiválasztható)</p>
                        <div className="upload-dropzone">
                            <input type="file" multiple onChange={handleGalleryChange} id="gallery-file-input" className="hidden-input" accept="image/png, image/jpeg" />
                            <label htmlFor="gallery-file-input" className="dropzone-content">
                                <span className="upload-icon">🎞️</span>
                                <p className="main-text">Kattintson ide a galéria képek hozzáadásához</p>
                            </label>
                        </div>
                        <div className="image-preview-gallery">
                            {galleryPreviews.map((img, index) => (
                                <div key={index} className="preview-item" style={{ backgroundImage: `url(${img})` }}></div>
                            ))}
                        </div>
                    </div>

                    <button type="submit" className="btn-submit-acc" disabled={isSubmitting}>
                        {isSubmitting ? 'Feltöltés folyamatban...' : 'Hirdetés közzététele'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default AddAccommodation;