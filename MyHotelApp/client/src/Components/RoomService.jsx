import '../css/RoomService.css';

export default function RoomService() {
  return (
    <form className="roomservice-form">
      <div className="form-group">
        <label className="form-label">Item Name:</label>
        <input type="text" name="itemName" className="form-input" />
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
          />
        </div>
      </div>

      <div className="form-group">
        <label className="form-label">Description:</label>
        <textarea
          name="description"
          className="form-input textarea-input"
          rows="4"
        ></textarea>
      </div>

      <button type="submit" className="form-button">Submit</button>
    </form>
  );
}
