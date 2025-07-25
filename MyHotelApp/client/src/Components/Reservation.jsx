import React, { useState } from 'react';
import '../css/Reservation.css';

export default function Reservation() {
  const [showRoomServices, setShowRoomServices] = useState(false);
  const [showExtraServices, setShowExtraServices] = useState(false);

  const roomServices = ['Breakfast', 'Laundry', 'Room Cleaning'];
  const extraServices = ['Spa Access', 'Airport Pickup', 'Gym'];

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
              {roomServices.map((service, index) => (
                <label key={index} className="checkbox-item">
                  <input type="checkbox" name="roomServices" value={service} />
                  {service}
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
              {extraServices.map((service, index) => (
                <label key={index} className="checkbox-item">
                  <input type="checkbox" name="extraServices" value={service} />
                  {service}
                </label>
              ))}
            </div>
          )}
        </div>
      </div>

      <button type="submit" className="form-button">Submit</button>
    </form>
  );
}
