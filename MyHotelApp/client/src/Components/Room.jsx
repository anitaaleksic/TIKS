import '../css/Room.css';

export default function Room() {
  return (
    <form className="room-form">
      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input type="text" name="roomNumber" className="form-input" />
      </div>

      <div className="form-group">
        <label className="form-label">Room Type:</label>
        <select name="roomType" className="form-input" defaultValue="Single">
          <option value="Single">Single</option>
          <option value="Double">Double</option>
          <option value="Suite">Suite</option>
          <option value="Deluxe">Deluxe</option>
        </select>
      </div>

      <div className="form-group">
        <label className="form-label">Capacity:</label>
        <input type="number" name="capacity" className="form-input" min="1" />
      </div>

      <div className="form-group">
        <label className="form-label">Floor:</label>
        <input type="number" name="floor" className="form-input" min="1" max="6" />
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
          />
        </div>
      </div>

      <div className="form-group">
        <label className="form-label">Image:</label>
        <input type="file" name="imageFile" className="form-input" accept="image/*" />
      </div>

      <button type="submit" className="form-button">Submit</button>
    </form>
  );
}
