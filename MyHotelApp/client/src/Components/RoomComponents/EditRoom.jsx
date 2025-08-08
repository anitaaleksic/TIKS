import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function EditRoom() {
  const { roomNumber } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    roomNumber: '',
    roomTypeID: '',
    floor: '',
    isAvailable: false
  });

  const [errorMessages, setErrorMessages] = useState([]);

  useEffect(() => {
    axios.get(`/api/Room/GetRoom/${roomNumber}`)
      .then(res => setFormData(res.data))
      .catch(err => {
        console.error(err);
        alert('Failed to load room data.');
        navigate('/room');
      });
  }, [roomNumber]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const errors = [];
    if (!formData.roomTypeID) errors.push('Room Type ID is required.');
    if (formData.floor === '') errors.push('Floor is required.');

    if (errors.length > 0) {
      setErrorMessages(errors);
      return;
    }

    axios.put(`/api/Room/UpdateRoom/${roomNumber}`, formData)
      .then(() => {
        alert('Room updated successfully!');
        navigate('/room');
      })
      .catch(err => {
        console.error(err);
        setErrorMessages([err.response?.data?.message || 'Update failed.']);
      });
  };

  return (
    <form className="extraservice-form" onSubmit={handleSubmit}>
      <h2>Edit Room</h2>

      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input
          className="form-input"
          name="roomNumber"
          type="text"
          value={formData.roomNumber}
          disabled
        />
      </div>

      <div className="form-group">
        <label className="form-label">Room Type ID:</label>
        <input
          className="form-input"
          name="roomTypeID"
          type="text"
          value={formData.roomTypeID}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Floor:</label>
        <input
          className="form-input"
          name="floor"
          type="number"
          value={formData.floor}
          onChange={handleChange}
        />
      </div>


      <button type="submit" className="form-button">Update Room</button>

      {errorMessages.length > 0 && (
        <div style={{ color: 'red', marginTop: '1rem' }}>
          <ul>
            {errorMessages.map((msg, idx) => <li key={idx}>{msg}</li>)}
          </ul>
        </div>
      )}
    </form>
  );
}
