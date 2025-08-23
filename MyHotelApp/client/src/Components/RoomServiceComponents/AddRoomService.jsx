import '../../css/Price.css'
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function AddRoomService() {
  const [formData, setFormData] = useState({
    itemName: '',
    itemPrice: '',
    description: ''
  });

  const [errorMessages, setErrorMessages] = useState([]);
  const navigate = useNavigate(); 
  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData(prev => ({
      ...prev,
      [name]: name === 'itemPrice' ? value.replace(',', '.') : value
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

    // Frontend validacija
    if (!formData.itemName || formData.itemName.length > 50) {
      setErrorMessages(['Item name is required and cannot exceed 50 characters.']);
      return;
    }

    if (formData.itemPrice === '' || isNaN(formData.itemPrice) || parseFloat(formData.itemPrice) <= 0) {
      setErrorMessages(['Price is required and must be a positive number.']);
      return;
    }

    const roomServiceToSend = {
      itemName: formData.itemName,
      itemPrice: parseFloat(formData.itemPrice),
      description: formData.description
    };

    axios.post('/api/RoomService/CreateRoomService', roomServiceToSend)
      .then(() => {
        alert('Room service added successfully!');
        setFormData({ itemName: '', itemPrice: '', description: '' });
        setErrorMessages([]);
        navigate("/roomservice");
      })
      .catch(err => {
        console.error('Error:', err.response || err);
        if (err.response?.data?.errors) {
          setErrorMessages(formatErrors(err.response.data.errors));
        } else {
          setErrorMessages([err.response?.data?.message || err.message]);
        }
      });
  };

  const handleExit = () => {
    navigate("/roomservice");
  }

  return (
    <form className="roomservice-form" onSubmit={handleSubmit}>
      <div className="form-group">
        <button type="button" className="exit-button" onClick={handleExit}>
          x
        </button>
        
        <label className="form-label">Item Name:</label>
        <input
          type="text"
          name="itemName"
          className="form-input"
          value={formData.itemName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        
        <label className="form-label">Item Price:</label>
        <div className="price-input-wrapper">
          <span className="currency-symbol">$</span>
          <input
            type="text"
            name="itemPrice"
            className="form-input price-input"
            placeholder="0.00"
            value={formData.itemPrice}
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
          maxLength={300}
          value={formData.description}
          onChange={handleChange}
        ></textarea>
      </div>

      <button type="submit" className="form-button">Add room service</button>

      {errorMessages.length > 0 && (
        <div className="error-messages" style={{ color: 'red', marginTop: '1rem' }}>
          <h4>Errors:</h4>
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
