import '../css/ExtraService.css';
import { useState } from 'react';
import axios from 'axios';

export default function ExtraService() {
  const [formData, setFormData] = useState({
    serviceName: '',
    price: '',
    description: ''
  });

  const [errorMessages, setErrorMessages] = useState([]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' ? value.replace(',', '.') : value
    }));
  };

  const formatErrors = (errorsObj) => {
    let messages = [];
    for (const field in errorsObj) {
      const errors = errorsObj[field];
      messages.push(`${field}: ${errors.join(', ')}`);
    }
    return messages;
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    // Provera pre slanja (frontend validacija)
    if (!formData.serviceName || formData.serviceName.length > 100) {
      setErrorMessages(['Naziv usluge je obavezan i mora imati manje od 100 karaktera.']);
      return;
    }

    if (formData.price === '' || isNaN(formData.price) || parseFloat(formData.price) <= 0) {
      setErrorMessages(['Cena mora biti pozitivan broj.']);
      return;
    }

    const extraServiceToSend = {
      serviceName: formData.serviceName,
      price: parseFloat(formData.price),
      description: formData.description
    };

    axios.post('/api/ExtraService/CreateExtraService', extraServiceToSend)
      .then(() => {
        alert('Usluga uspešno dodata!');
        setFormData({ serviceName: '', price: '', description: '' });
        setErrorMessages([]);
      })
      .catch(err => {
        console.error('Greška:', err.response || err);
        if (err.response?.data?.errors) {
          setErrorMessages(formatErrors(err.response.data.errors));
        } else {
          setErrorMessages([err.response?.data?.message || err.message]);
        }
      });
  };

  return (
    <form className="extraservice-form" onSubmit={handleSubmit}>
      <div className="form-group">
        <label className="form-label">Item Name:</label>
        <input
          type="text"
          name="serviceName"
          className="form-input"
          value={formData.serviceName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Item Price:</label>
        <div className="price-input-wrapper">
          <span className="currency-symbol">$</span>
          <input
            type="text"
            name="price"
            className="form-input price-input"
            placeholder="0.00"
            value={formData.price}
            onChange={handleChange}
          />
        </div>
      </div>

      <div className="form-group">
        <label className="form-label">Description:</label>
        <textarea
          name="description"
          className="form-input textarea-input"
          rows="4"
          value={formData.description}
          onChange={handleChange}
        ></textarea>
      </div>

      <button type="submit" className="form-button">Dodaj uslugu</button>

      {errorMessages.length > 0 && (
        <div className="error-messages" style={{ color: 'red', marginTop: '1rem' }}>
          <h4>Greške:</h4>
          <ul>
            {errorMessages.map((msg, idx) => (
              <li key={idx}>{msg}</li>
            ))}
          </ul>
        </div>
      )}
    </form>
  );
}
