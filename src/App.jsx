import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Landing-page/Navbar/Navbar';
import Hero from './components/Landing-page/Hero/Hero';
import Accommodation from './components/Landing-page/Accommodation/Accommodation';
import AddAccommodation from './components/Landing-page/Accommodation/AddAccommodation';
import AccommodationSearch from './components/Landing-page/Accommodation/AccommodationSearch';
import AccommodationDetails from './components/Landing-page/Accommodation/AccommodationDetails';
import Newsletter from './components/Landing-page/Newsletter/Newsletter';
import Footer from './components/Landing-page/Footer/Footer';
import Login from './components/Login/Login';
import ProfilePage from './components/ProfilePage/Profile'; 
import PaymentPage from './components/Landing-page/Accommodation/PaymentPage';
import './i18n'

import './App.css';

// A főoldal összetevői
const LandingPage = () => (
  <>
    <Hero />
    <Accommodation />
    <Newsletter />
  </>
);

function App() {
  return (
    <Router>
      <div className="App">
        <Navbar />
        
        <main>
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/login" element={<Login />} />
              
            <Route path="/profile" element={<ProfilePage />} />
            
            <Route path="/szallasok" element={<AccommodationSearch />} />
            
            <Route path="/add-accommodation" element={<AddAccommodation />} />
            <Route path="/accommodation/:id" element={<AccommodationDetails />} />
            <Route path="/payment/:id" element={<PaymentPage />} />
          </Routes>
        </main>

        <Footer />
      </div>
    </Router>
  );
}

export default App;