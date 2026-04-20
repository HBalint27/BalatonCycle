import React, { useState } from 'react';
import { Link } from 'react-router-dom'; 
import { useTranslation } from 'react-i18next'; // 1. Import hook
import './Login.css';

const Login = () => {
    const { t } = useTranslation(); // 2. Initialize hook
    const [isLogin, setIsLogin] = useState(true);
    const [errors, setErrors] = useState({});
    
    const [formData, setFormData] = useState({
        email: '',
        password: '',
        confirmPassword: '',
        name: '',
        address: '',
        phone: ''
    });

    const patterns = {
        name: /^[A-ZÁÉÍÓÖŐÚÜŰ][a-záéíóöőúüű]+(\s[A-ZÁÉÍÓÖŐÚÜŰ][a-záéíóöőúüű]+)+$/,
        email: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
        password: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$/,
        phone: /^(36)(20|30|31|70)\d{7}$/,
        address: /^[A-ZÁÉÍÓÖŐÚÜŰa-záéíóöőúüű0-9\s,.\-/]{2,100}$/
    };

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
        if (errors[e.target.name]) {
            setErrors({ ...errors, [e.target.name]: "" });
        }
    };

    const validate = () => {
        let tempErrors = {};

        if (!isLogin) {
            if (!patterns.name.test(formData.name)) tempErrors.name = t('auth.errors.name');
            if (!patterns.address.test(formData.address)) tempErrors.address = t('auth.errors.address');
            if (!patterns.phone.test(formData.phone.replace(/\s/g, ""))) tempErrors.phone = t('auth.errors.phone');
            if (formData.password !== formData.confirmPassword) tempErrors.confirmPassword = t('auth.errors.match');
        }

        if (!patterns.email.test(formData.email)) tempErrors.email = t('auth.errors.email');
        if (!patterns.password.test(formData.password)) tempErrors.password = t('auth.errors.password_req');

        setErrors(tempErrors);
        return Object.keys(tempErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validate()) return;

        const API_URL = "https://localhost:7284/api"; 

        try {   
            if (isLogin) {
                const response = await fetch(`${API_URL}/login`, { 
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        Email: formData.email,
                        Password: formData.password 
                    }) 
                });

                if (response.ok) {
                    const data = await response.json(); 
                    localStorage.setItem("token", data.token);
                    localStorage.setItem("user", JSON.stringify(data.user));
                    window.location.href = "/"; 
                } else {
                    setErrors({ server: t('auth.errors.server_fail') });
                }

            } else {
                const regAdat = {
                    nev: formData.name,
                    email: formData.email,
                    jelszo: formData.password,
                    lakcim: formData.address,
                    telefonszam: formData.phone.replace(/\D/g, ""),
                    statusz: "User"
                };

                const response = await fetch(`${API_URL}/Felhasznalo`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(regAdat)
                });

                if (response.ok) {
                    alert(t('auth.errors.reg_success'));
                    setIsLogin(true);
                } else {
                    const errorMsg = await response.text();
                    setErrors({ server: errorMsg || "Error!" });
                }
            }
        } catch (err) {
            setErrors({ server: t('auth.errors.no_server') });
        }
    };

    return (
        <div className="main-wrapper">
            <div className="auth-card">
                <div className="auth-header">
                    <div className="logo-text">BALATON<span>CYCLE</span></div>
                    <p>{t('auth.welcome')}</p>
                </div>

                <div className="form-toggle">
                    <button className={isLogin ? "active" : ""} onClick={() => { setIsLogin(true); setErrors({}); }}>
                        {t('auth.login_tab')}
                    </button>
                    <button className={!isLogin ? "active" : ""} onClick={() => { setIsLogin(false); setErrors({}); }}>
                        {t('auth.register_tab')}
                    </button>
                </div>

                <div className="form-container-inner">
                    <form className="auth-form" onSubmit={handleSubmit} noValidate>
                        <h2>{isLogin ? t('auth.login_title') : t('auth.register_title')}</h2>
                        
                        {!isLogin && (
                            <div className="input-group">
                                <input 
                                    type="text" 
                                    name="name" 
                                    placeholder={t('auth.fields.name')} 
                                    onChange={handleChange} 
                                    className={errors.name ? "input-error" : ""} 
                                />
                                {errors.name && <span className="field-error">{errors.name}</span>}
                            </div>
                        )}

                        <div className="input-group">
                            <input 
                                type="email" 
                                name="email" 
                                placeholder={t('auth.fields.email')} 
                                onChange={handleChange} 
                                className={errors.email ? "input-error" : ""} 
                            />
                            {errors.email && <span className="field-error">{errors.email}</span>}
                        </div>

                        <div className="input-group">
                            <input 
                                type="password" 
                                name="password" 
                                placeholder={t('auth.fields.password')} 
                                onChange={handleChange} 
                                className={errors.password ? "input-error" : ""} 
                            />
                            {errors.password && <span className="field-error">{errors.password}</span>}
                        </div>

                        {!isLogin && (
                            <>
                                <div className="input-group">
                                    <input 
                                        type="password" 
                                        name="confirmPassword" 
                                        placeholder={t('auth.fields.confirm_password')} 
                                        onChange={handleChange} 
                                        className={errors.confirmPassword ? "input-error" : ""} 
                                    />
                                    {errors.confirmPassword && <span className="field-error">{errors.confirmPassword}</span>}
                                </div>
                                <div className="input-group">
                                    <input 
                                        type="text" 
                                        name="address" 
                                        placeholder={t('auth.fields.city')} 
                                        onChange={handleChange} 
                                        className={errors.address ? "input-error" : ""} 
                                    />
                                    {errors.address && <span className="field-error">{errors.address}</span>}
                                </div>
                                <div className="input-group">
                                    <input 
                                        type="tel" 
                                        name="phone" 
                                        maxLength={11} 
                                        placeholder={t('auth.fields.phone')} 
                                        onChange={handleChange} 
                                        className={errors.phone ? "input-error" : ""} 
                                    />
                                    {errors.phone && <span className="field-error">{errors.phone}</span>}
                                </div>
                            </>
                        )}

                        {errors.server && <p className="server-error">{errors.server}</p>}

                        <button type="submit" className="btn-primary">
                            {isLogin ? t('auth.login_tab') : t('auth.register_tab')}
                        </button>
                    </form>                    
                </div>
                
                <div className="auth-footer">                    
                    <Link to="/" className="back-link">
                        {t('auth.back_home')}
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default Login;