import '../css/Room.css';
import { useState } from 'react';
import axios from 'axios';

export default function Room() {
  const [formData, setFormData] = useState({
    roomNumber: '',
    roomType: 0,
    capacity: 1,
    floor: '', // automatski se računa
    pricePerNight: '',
    isAvailable: true,
    imageUrl: ''
  });

  const [errorMessages, setErrorMessages] = useState([]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === 'roomNumber') {
      const roomNum = Number(value);
      const floorAuto = Math.floor(roomNum / 100);

      setFormData(prev => ({
        ...prev,
        roomNumber: roomNum,
        floor: isNaN(floorAuto) ? '' : floorAuto
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: ['capacity', 'roomType'].includes(name)
          ? Number(value)
          : value
      }));
    }
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

    if (
      formData.roomNumber < 101 ||
      formData.roomNumber > 699
    ) {
      setErrorMessages(['Broj sobe mora biti između 101 i 699.']);
      return;
    }

    if (formData.pricePerNight === '' || isNaN(formData.pricePerNight)) {
      setErrorMessages(['Cena mora biti validan broj.']);
      return;
    }

    const roomToSend = {
      ...formData,
      pricePerNight: parseFloat(formData.pricePerNight)
    };

    console.log('Šaljem na backend:', roomToSend);

    axios.post('/api/Room/CreateRoom', roomToSend)
      .then(() => {
        alert('Soba uspešno dodata!');
        setFormData({
          roomNumber: '',
          roomType: 0,
          capacity: 1,
          floor: '',
          pricePerNight: '',
          isAvailable: true,
          imageUrl: ''
        });
        setErrorMessages([]);
      })
      .catch(err => {
        console.error('Greška pri dodavanju sobe:', err.response || err);
        if (err.response?.data?.errors) {
          setErrorMessages(formatErrors(err.response.data.errors));
        } else {
          setErrorMessages([err.response?.data?.message || err.message]);
        }
      });
  };

  return (
    <form className="room-form" onSubmit={handleSubmit} noValidate>
      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input
          type="number"
          name="roomNumber"
          className="form-input"
          min="101"
          max="699"
          value={formData.roomNumber}
          onChange={handleChange}
          required
        />
      </div>

      <div className="form-group">
        <label className="form-label">Room Type:</label>
        <select
          name="roomType"
          className="form-input"
          value={formData.roomType}
          onChange={handleChange}
        >
          <option value={0}>Single</option>
          <option value={1}>Double</option>
          <option value={2}>Suite</option>
          <option value={3}>Deluxe</option>
        </select>
      </div>

      <div className="form-group">
        <label className="form-label">Capacity:</label>
        <input
          type="number"
          name="capacity"
          className="form-input"
          min="1"
          max="5"
          value={formData.capacity}
          onChange={handleChange}
          required
        />
      </div>

      <div className="form-group">
        <label className="form-label">Floor (auto):</label>
        <input
          type="number"
          className="form-input"
          value={formData.floor}
          readOnly
        />
      </div>

      <div className="form-group">
        <label className="form-label">Price Per Night:</label>
        <div className="price-input-wrapper">
          <span className="currency-symbol">$</span>
          <input
            type="text"
            name="pricePerNight"
            className="form-input price-input"
            placeholder="0.00"
            value={formData.pricePerNight}
            onChange={handleChange}
            required
          />
        </div>
      </div>

      <div className="form-group">
        <label className="form-label">Image URL:</label>
        <input
          type="text"
          name="imageUrl"
          className="form-input"
          placeholder="https://..."
          value={formData.imageUrl}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Dodaj sobu</button>

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
