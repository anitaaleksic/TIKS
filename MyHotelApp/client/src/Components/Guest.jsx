import '../css/Guest.css';
import { useState } from 'react';
import axios from 'axios';

export default function Guest() {
  const [formData, setFormData] = useState({
    fullName: '',
    jmbg: '',
    phoneNumber: ''
  });

  const handleChange = (e) => {
    setFormData(prev => ({
      ...prev,
      [e.target.name]: e.target.value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    axios.post('/api/Guest/CreateGuest', formData)
    .then(res => {
      console.log('Server je vratio:', res.data);
      alert('Gost dodat!');
      setFormData({ fullName: '', jmbg: '', phoneNumber: '' }); // resetuj formu
    })
    .catch(err => {
      console.error('Greška pri pozivu API-ja:', err.response || err);
      alert('Greška pri dodavanju gosta: ' + (err.response?.data?.message || err.message));
    });

  };

  return (
    <form className="guest-form" onSubmit={handleSubmit}>
      <div className="form-group">
        <label className="form-label" htmlFor="fullName">Full Name:</label>
        <input
          type="text"
          id="fullName"
          name="fullName"
          className="form-input"
          value={formData.fullName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label" htmlFor="jmbg">JMBG:</label>
        <input
          type="text"
          id="jmbg"
          name="jmbg"
          className="form-input"
          value={formData.jmbg}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label" htmlFor="phoneNumber">Phone Number:</label>
        <input
          type="text"
          id="phoneNumber"
          name="phoneNumber"
          className="form-input"
          placeholder="+381..."
          value={formData.phoneNumber}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Dodaj gosta</button>
    </form>
  );
}
