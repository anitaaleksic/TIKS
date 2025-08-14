import { useState, useEffect} from 'react';
import axios from 'axios';

export default function AddRoom() {
  const [formData, setFormData] = useState({
    roomNumber: '',
    roomTypeID: 0,
    floor: '', // automatski se računa
    isAvailable: true,
  });

  const [errorMessages, setErrorMessages] = useState([]);
  const [roomTypes, setRoomTypes] = useState([]);

  useEffect(() => {
    axios.get('/api/RoomType/GetAllRoomTypes')
      .then(res => setRoomTypes(res.data))
      .catch(err => console.error('An error has occurred while loading RoomType', err));
  }, []);

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
        [name]: name === 'roomTypeID' ? Number(value) : value
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

    if (formData.roomNumber < 101 || formData.roomNumber > 699) {
      setErrorMessages(['Room number is required and must be between 1 and 699.']);
      return;
    }

    if(!formData.roomTypeID) {
      setErrorMessages(['Room type is required.']);
      return;
    }

    const roomToSend = {
      ...formData,
    };

    //console.log('Šaljem na backend:', roomToSend);

    axios.post('/api/Room/CreateRoom', roomToSend)
      .then(() => {
        alert('Room added successfully!');
        setFormData({
          roomNumber: '',
          roomType: 0,
          floor: '',
          isAvailable: true,
        });
        setErrorMessages([]);
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
        name="roomTypeID"
        className="form-input"
        value={formData.roomTypeID}
        onChange={handleChange}
      >
        <option value="">Choose Room Type</option>
        {roomTypes.map((type) => (
          <option key={type.roomTypeID} value={type.roomTypeID}>
            {type.type}
          </option>
        ))}
      </select>
      </div>

      <div className="form-group">
        <label className="form-label">Floor:</label>
        <input
          type="number"
          className="form-input"
          value={formData.floor}
          readOnly
        />
      </div>

      <button type="submit" className="form-button">Add room</button>

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
