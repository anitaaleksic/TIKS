import React, { useState, useEffect } from 'react';
import '../../css/Reservation.css'
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
 
export default function AddReservation() {
  const [showRoomServices, setShowRoomServices] = useState(false);
  const [showExtraServices, setShowExtraServices] = useState(false);
  const [roomServices, setRoomServices] = useState([]);
  const [extraServices, setExtraServices] = useState([]);
  const [guestExists, setGuestExists] = useState(null);
  const [guestID, setGuestID] = useState('');
  const [roomExists, setRoomExists] = useState(null);
  const [roomAvailable, setRoomAvailable] = useState(null);
  const [totalPrice, setTotalPrice] = useState(0);
  const [errorMessages, setErrorMessages] = useState([]);

  const [formData, setFormData] = useState({
    roomNumber: '',
    guestID: '',    
    checkInDate: '',
    checkOutDate: '',
    roomServiceIDs: [],
    extraServiceIDs: []
  });

  const navigate = useNavigate();

  const checkGuestExists = async (guestID) => {
    if(!guestID || guestID.length !== 13){
      setGuestExists(null);
      setGuestID(null);
      return;
    }
    try {
      const res = await axios.get(`api/Guest/GetGuestByJMBG/${guestID}`);
      setGuestExists(!!res.data);
      if(res.data) setGuestID(guestID);
      else setGuestID(null);
    } catch(err) {
      console.error('An error occurred checking JMBG:', err);
      setGuestExists(null);
      setGuestID(null);
    }
  };

  const checkRoomExists = async (roomNumber) => {
    if(!roomNumber || roomNumber < 101 || roomNumber > 699){
      setRoomExists(null);
      return;
    }
    try {
      const res = await axios.get(`api/Room/GetRoom/${roomNumber}`);
      setRoomExists(!!res.data);
    } catch(err){
      console.error('An error occurred checking room number:', err);
      setRoomExists(null);
    }
  };

  const checkRoomAvailability = async () => {
    const { roomNumber, checkInDate, checkOutDate} = formData;
    if(!roomNumber || !checkInDate || !checkOutDate) return;
    try {
      const res = await axios.get(`/api/Reservation/IsRoomAvailable/${roomNumber}/${checkInDate}/${checkOutDate}`);
      setRoomAvailable(res.data.available);
    } catch(err){
      console.error('Error occurred checking room availability:', err);
      setRoomAvailable(null);
    }
  }

  const handleRoomServiceChange = (rsId) => {
    setFormData(prev => {
      const selected = prev.roomServiceIDs.includes(rsId)
            ? prev.roomServiceIDs.filter(id => id !== rsId)
            : [...prev.roomServiceIDs, rsId];
      return { ...prev, roomServiceIDs: selected}
    });
  };

  const handleExtraServiceChange = (esId) => {
    setFormData(prev => {
      const selected = prev.extraServiceIDs.includes(esId)
            ? prev.extraServiceIDs.filter(id => id !== esId)
            : [...prev.extraServiceIDs, esId];
      return { ...prev, extraServiceIDs: selected}
    });
  }

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
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
    const checkIn = new Date(formData.checkInDate);
    const checkOut = new Date(formData.checkOutDate);

    if (checkOut <= checkIn) {
      setErrorMessages(["Check-out date must be after check-in date."]);
      return;
    }
    if(guestExists === false){
      setErrorMessages(["Guest with that JMBG doesn't exist."]);
      return;
    }
    if(roomExists === false){
      setErrorMessages(["Room with that number doesn't exist."]);
      return;
    }
    if(roomAvailable === false){
      setErrorMessages(["Room isn't available for selected dates."]);
      return;
    }

    axios.post('/api/Reservation/CreateReservation', {
      ...formData,
      totalPrice: totalPrice
    })
    .then(() => {
      alert('Reservation added successfully!');
      setFormData({ roomNumber: '', guestID: '', checkInDate: '', checkOutDate: '', roomServiceIDs: [], extraServiceIDs: [] });
      setErrorMessages([]);
      navigate("/reservation");
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

  // Load room & extra services
  useEffect(() => {
    axios.get('/api/RoomService/GetAllRoomServices')
      .then(res => setRoomServices(res.data))
      .catch(err => console.error('An error occurred loading Room Service', err));

    axios.get('/api/ExtraService/GetAllExtraServices')
      .then(res => setExtraServices(res.data))
      .catch(err => console.error('An error occurred loading Extra Service', err));

    if(formData.guestID) checkGuestExists(formData.guestID);
    if(formData.roomNumber) checkRoomExists(formData.roomNumber);
    if(formData.roomNumber && formData.checkInDate && formData.checkOutDate)
      checkRoomAvailability();

  }, [formData.guestID, formData.checkInDate, formData.checkOutDate, formData.roomServiceIDs, formData.extraServiceIDs, formData.roomNumber]);

  // Calculate total price
  useEffect(() => {
    const calculateTotal = async () => {
      if(!formData.checkInDate || !formData.checkOutDate || !formData.roomNumber) return;

      const days = (new Date(formData.checkOutDate) - new Date(formData.checkInDate)) / (1000 * 60 * 60 * 24);
      if(days <= 0) return;

      let total = 0;

      try {
        // Get room to fetch RoomTypeID
        const roomRes = await axios.get(`/api/Room/GetRoom/${formData.roomNumber}`);
        const roomTypeID = roomRes.data.roomTypeID;

        // Get price per night from RoomType
        const roomTypeRes = await axios.get(`/api/RoomType/GetRoomTypeById/${roomTypeID}`);
        const pricePerNight = roomTypeRes.data.pricePerNight || 0;
        total += pricePerNight * days;
      } catch(err){
        console.error('Room price fetch failed:', err);
      }

      // Add room services
      for(let id of formData.roomServiceIDs) {
        const service = roomServices.find(s => s.roomServiceID === id);
        if(service) total += parseFloat(service.itemPrice || 0);
      }

      // Add extra services
      for(let id of formData.extraServiceIDs) {
        const service = extraServices.find(s => s.extraServiceID === id);
        if(service) total += parseFloat(service.price || 0) * days;
      }

      const roundedTotal = parseFloat(total.toFixed(2));
      if(roundedTotal !== totalPrice) setTotalPrice(roundedTotal);
    };

    calculateTotal();
  }, [formData.checkInDate, formData.checkOutDate, formData.roomNumber, formData.roomServiceIDs, formData.extraServiceIDs, roomServices, extraServices, totalPrice]);

  const handleExit = () => navigate("/reservation");

  return (
    <form className="reservation-form" onSubmit={handleSubmit}>
      <button type="button" className="exit-button" onClick={handleExit}>x</button>

      <div className="form-group">
        <label className="form-label">Guest JMBG:</label>
        <input type="text" name="guestID" className="form-input" value={formData.guestID} onChange={handleChange} onBlur={() => checkGuestExists(formData.guestID)} />
        {guestExists === false && <span className="form-error">Guest with that JMBG doesn't exist.</span>}
      </div>

      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input type="text" name="roomNumber" className="form-input" value={formData.roomNumber} onChange={handleChange} onBlur={() => checkRoomExists(formData.roomNumber)} />
        {roomExists === false && <span className="form-error">Room with that number doesn't exist.</span>}
      </div>

      <div className="form-group">
        <label className="form-label">Check-In Date:</label>
        <input type="date" name="checkInDate" className="form-input" value={formData.checkInDate} onChange={handleChange} />
        {roomAvailable === false && <span className="form-error">Room is not available for selected dates.</span>}
      </div>

      <div className="form-group">
        <label className="form-label">Check-Out Date:</label>
        <input type="date" name="checkOutDate" className="form-input" value={formData.checkOutDate} onChange={handleChange} />
        {roomAvailable === false && <span className="form-error">Room is not available for selected dates.</span>}
      </div>

      <div className="button-pair-row">
        <div className="toggle-group-inline">
          <button type="button" className="form-button" onClick={() => setShowRoomServices(!showRoomServices)}>Room Services</button>
          {showRoomServices && (
            <div className="checkbox-group" data-testid="room-services">
              <label className="form-label">Room Services:</label>
              {roomServices.map(s => (
                <label key={s.roomServiceID} className="checkbox-item">
                  <input type="checkbox" value={s.roomServiceID} onChange={() => handleRoomServiceChange(s.roomServiceID)} />
                  {s.itemName}
                </label>
              ))}
            </div>
          )}
        </div>

        <div className="toggle-group-inline">
          <button type="button" className="form-button" onClick={() => setShowExtraServices(!showExtraServices)}>Extra Services</button>
          {showExtraServices && (
            <div className="checkbox-group" data-testid="extra-services">
              <label className="form-label">Extra Services:</label>
              {extraServices.map(s => (
                <label key={s.extraServiceID} className="checkbox-item">
                  <input type="checkbox" value={s.extraServiceID} onChange={() => handleExtraServiceChange(s.extraServiceID)} />
                  {s.serviceName}
                </label>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="form-group">
        <label className="form-label">Total Price:</label>
        <div className="price-input-wrapper">
          <span className="currency-symbol">$</span>
          <input type="text" name="totalPrice" className="form-input price-input" placeholder="0.00" value={totalPrice} readOnly />
        </div>
      </div>

      <button type="submit" className="form-button">Add reservation</button>

      {errorMessages.length > 0 && (
        <div className="error-messages" style={{ color: 'red', marginTop: '1rem' }}>
          <h4>Errors:</h4>
          <ul>
            {errorMessages.map((msg, idx) => <li key={idx}>{msg}</li>)}
          </ul>
        </div>
      )}
    </form>
  );
}
