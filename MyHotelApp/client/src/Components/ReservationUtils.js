import axios from 'axios';

export const checkGuestExists = async (guestID, setGuestExists, setGuestID) => {
    if(!guestID || guestID.length !== 13){
      setGuestExists(null);
      setGuestID(null);//jer se na back-u zove GuestId a ne jmbg....
      return;
    }
    try {
      const res = await axios.get(`api/Guest/GetGuestByJMBG/${guestID}`);
      if(res.data){
        setGuestExists(true);
        setGuestID(guestID);
      }
      else {
        setGuestExists(null);
        setGuestID(null);
      }
    }
    catch(err) {
      console.error('An error occurred checking JMBG:', err);
      setGuestExists(null);
      setGuestID(null);
    }
  };

  export const checkRoomExists = async (roomNumber, setRoomExists) => {
    if(!roomNumber || roomNumber < 101 || roomNumber > 699){
      setRoomExists(null);
      return;
    }
    try {
      const res = await axios.get(`api/Room/GetRoom/${roomNumber}`);
      setRoomExists(res.data.exists);
    }
    catch(err){
      console.error('An error occurred checking room number:', err);
      setRoomExists(null);
    }
  };

  export const checkRoomAvailability = async (formData, setRoomAvailable) => {
    const { roomNumber, checkInDate, checkOutDate} = formData;
    if(!roomNumber || !checkInDate || !checkOutDate) 
      return;

    try {
      const res = await axios.get(`/api/Reservation/IsRoomAvailable/${roomNumber}/${checkInDate}/${checkOutDate}`);
      setRoomAvailable(res.data.available);
    }
    catch(err){
      console.error('Error occurred checking room availability:', err);
      setRoomAvailable(null);
    }
  }

  export const handleRoomServiceChange = (rsId, setFormData) => {
    setFormData(prev => {
      const selected = prev.roomServiceIDs.includes(rsId) 
            ? prev.roomServiceIDs.filter(id => id !== rsId)
            : [...prev.roomServiceIDs, rsId];
      return { ...prev, roomServiceIDs: selected}
    });
  };

  export const handleExtraServiceChange = (esId, setFormData) => {
    setFormData(prev => {
      const selected = prev.extraServiceIDs.includes(esId)
            ? prev.extraServiceIDs.filter(id => id !== esId)
            : [...prev.extraServiceIDs, esId];
      return { ...prev, extraServiceIDs: selected}
    })
  }
