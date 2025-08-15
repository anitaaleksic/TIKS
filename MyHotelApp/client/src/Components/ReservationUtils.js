import axios from 'axios';

// const [guestExists, setGuestExists] = useState([]);
// const [guestID, setGuestID] = useState('');
// const [roomExists, setRoomExists] = useState([]);
// const [roomAvailable, setRoomAvailable] = useState([]);

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

//   const handleRoomServiceChange = (rsId) => {
//     setFormData(prev => {
//       const selected = prev.roomServiceIDs.includes(rsId) 
//             ? prev.roomServiceIDs.filter(id => id !== rsId)
//             : [...prev.roomServiceIDs, rsId];
//       return { ...prev, roomServiceIDs: selected}
//     });
//   };

//   const handleExtraServiceChange = (esId) => {
//     setFormData(prev => {
//       const selected = prev.extraServiceIDs.includes(esId)
//             ? prev.extraServiceIDs.filter(id => id !== esId)
//             : [...prev.extraServiceIDs, esId];
//       return { ...prev, extraServiceIDs: selected}
//     })
//   }

//   const handleChange = (e) => {
//     const { name, value } = e.target;

//     setFormData(prev => ({
//       ...prev,
//       [name]: value
//     }));
//   };

//   const formatErrors = (errorsObj) => {
//     let messages = [];
//     for (const field in errorsObj) {
//       const errors = errorsObj[field];
//       messages.push(`${field}: ${errors.join(', ')}`);
//     }
//     return messages;
//   };

//   const handleSubmit = (e) => {
//     e.preventDefault();

//     const checkIn = new Date(formData.checkInDate);
//     const checkOut = new Date(formData.checkOutDate);

//     if (checkOut <= checkIn) {
//       setErrorMessages(["Check-out date must be after check-in date."]);
//       return;
//     }

//     if(guestExists === false){
//       setErrorMessages(["Guest with that JMBG doens't exist."]);
//       return;
//     }
//     if(roomExists === false){
//       setErrorMessages(["Room with that number doens't exist."]);
//       return;
//     }

//     if(roomAvailable === false){
//       setErrorMessages(["Room isn't available for selected dates."]);
//       return;
//     }

//     axios.post('/api/Reservation/CreateReservation', {
//           ...formData,
//           totalPrice: totalPrice
//       })
//       .then(() => {
//         alert('Reservation added successfully!');
//         setFormData({ roomNumber: '',
//                       guestID: '',    
//                       checkInDate: '', 
//                       checkOutDate: '',
//                       roomServiceIDs: [],
//                       extraServiceIDs: []});
//         setErrorMessages([]);
//       })
//       .catch(err => {
//         console.error('Error:', err.response || err);
//         if (err.response?.data?.errors) {
//           setErrorMessages(formatErrors(err.response.data.errors));
//         } else {
//           setErrorMessages([err.response?.data?.message || err.message]);
//         }
//       });
//   };