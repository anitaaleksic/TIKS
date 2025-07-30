import React, { useState, useEffect } from 'react';
import '../css/Reservation.css';
import axios from 'axios';

export default function Reservation() {
  
  const [showRoomServices, setShowRoomServices] = useState(false);
  const [showExtraServices, setShowExtraServices] = useState(false);
  const [roomServices, setRoomServices] = useState([]);
  const [extraServices, setExtraServices] = useState([]);

  useEffect(() => {
    axios.get('/api/RoomService/GetAllRoomServices')
      .then(res => setRoomServices(res.data))
      .catch(err => console.error('Greška pri učitavanju RoomService:', err));

    axios.get('/api/ExtraService/GetAllExtraServices')
      .then(res => setExtraServices(res.data))
      .catch(err => console.error('Greška pri učitavanju ExtraService:', err));
  }, []);

  return (
    <form className="reservation-form">
      <div className="form-group">
        <label className="form-label">Guest JMBG:</label>
        <input type="text" name="guestJMBG" className="form-input" />
      </div>

      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input type="text" name="roomNumber" className="form-input" />
      </div>

      <div className="form-group">
        <label className="form-label">Check-In Date:</label>
        <input type="date" name="checkInDate" className="form-input" />
      </div>

      <div className="form-group">
        <label className="form-label">Check-Out Date:</label>
        <input type="date" name="checkOutDate" className="form-input" />
      </div>


      <div className="button-pair-row">
        <div className="toggle-group-inline">
          <button
            type="button"
            className="form-button1"
            onClick={() => setShowRoomServices(!showRoomServices)}
          >
            Room Services
          </button>
          {showRoomServices && (
            <div className="checkbox-group">
              <label className="form-label">Room Services:</label>
              {roomServices.map((service) => (
                <label key={service.roomServiceID} className="checkbox-item">
                  <input type="checkbox" name="roomServices" value={service.roomServiceID} />
                  {service.itemName}
                </label>
              ))}
            </div>
          )}
        </div>

        <div className="toggle-group-inline">
          <button
            type="button"
            className="form-button1"
            onClick={() => setShowExtraServices(!showExtraServices)}
          >
            Extra Services
          </button>
          {showExtraServices && (
            <div className="checkbox-group">
              <label className="form-label">Extra Services:</label>
              {extraServices.map((service) => (
                <label key={service.extraServiceID} className="checkbox-item">
                  <input type="checkbox" name="extraServices" value={service.extraServiceID} />
                  {service.serviceName}
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
          <input
            type="text"
            name="totalPrice"
            className="form-input price-input"
            placeholder="0.00"
            readOnly
          />
        </div>
      </div>
      
      <button type="submit" className="form-button">Submit</button>
    </form>
  );
}
