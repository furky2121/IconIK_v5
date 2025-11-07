'use client';

import React, { useEffect, useRef, useState, useImperativeHandle, forwardRef } from 'react';
import { Button } from 'primereact/button';
import { ProgressBar } from 'primereact/progressbar';
import { Toast } from 'primereact/toast';
import { Card } from 'primereact/card';
import { Badge } from 'primereact/badge';
import { Divider } from 'primereact/divider';
import videoEgitimService from '../services/videoEgitimService';
// CSS import removed to prevent production build issues

const VideoPlayer = forwardRef(({ egitim, onComplete, onProgress, personelId }, ref) => {
    const videoRef = useRef(null);
    const toast = useRef(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [progress, setProgress] = useState(0);
    const [volume, setVolume] = useState(1);
    const [isFullscreen, setIsFullscreen] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [watchedPercentage, setWatchedPercentage] = useState(0);
    const [isCompleted, setIsCompleted] = useState(false);
    const [savedProgress, setSavedProgress] = useState(null);
    const [isMounted, setIsMounted] = useState(false);
    const [isProcessingCompletion, setIsProcessingCompletion] = useState(false);
    const [hasShownCompletionToast, setHasShownCompletionToast] = useState(false);
    

    // Progress tracking intervals
    const progressIntervalRef = useRef(null);
    const saveProgressIntervalRef = useRef(null);
    
    // YouTube/Vimeo player states
    const [youTubePlayer, setYouTubePlayer] = useState(null);
    const [videoPlatform, setVideoPlatform] = useState('Local');
    const [playerReady, setPlayerReady] = useState(false);

    // Effect to set mounted state
    useEffect(() => {
        setIsMounted(true);
            // console.log(`VideoPlayer mounted - egitim: ${egitim?.id}, personel: ${personelId}, states: isCompleted=${isCompleted}, hasShownCompletionToast=${hasShownCompletionToast}`);

        // Clear any session completion data when component mounts with fresh data
        if (egitim?.id && personelId) {
            const completionKey = `video_completion_${egitim.id}_${personelId}`;
            // console.log(`Clearing session completion key: ${completionKey}`);
            sessionStorage.removeItem(completionKey);
        }

        return () => setIsMounted(false);
    }, [egitim?.id, personelId]);



    useEffect(() => {
        // Only run after component is mounted to avoid SSR issues
        if (!isMounted) return;

        // Detect video platform
        if (egitim?.videoUrl) {
            // console.log('🎬 VideoPlayer - Video URL detected:', egitim.videoUrl);
            
            if (egitim.videoUrl.includes('youtube.com') || egitim.videoUrl.includes('youtu.be')) {
            // console.log('📺 VideoPlayer - Detected YouTube video');
                setVideoPlatform('YouTube');
                loadYouTubeAPI();
            } else if (egitim.videoUrl.includes('vimeo.com')) {
            // console.log('📹 VideoPlayer - Detected Vimeo video');
                setVideoPlatform('Vimeo');
                loadVimeoAPI();
            } else {
            // console.log('💻 VideoPlayer - Detected Local video');
                setVideoPlatform('Local');
            }
        } else {
            // console.log('❌ VideoPlayer - No video URL found in egitim data');
        }

        // Load saved progress when component mounts
        if (egitim?.id && personelId) {
            loadSavedProgress();
        }

        return () => {
            // Save progress before component unmounts
            if (egitim?.id && personelId && watchedPercentage > 0 && !isCompleted) {
            // console.log('Component unmounting, saving current progress...');
                saveCurrentProgress();
            }

            // Cleanup intervals
            if (progressIntervalRef.current) {
                clearInterval(progressIntervalRef.current);
            }
            if (saveProgressIntervalRef.current) {
                clearInterval(saveProgressIntervalRef.current);
            }

            // Cleanup YouTube player
            if (youTubePlayer && youTubePlayer.destroy) {
                try {
                    youTubePlayer.destroy();
                } catch (error) {
            // console.log('YouTube player cleanup error:', error);
                }
            }
        };
    }, [isMounted, egitim?.id, egitim?.videoUrl, personelId]);

    // Initialize players when API is ready
    useEffect(() => {
        if (!isMounted) return;
        
        if (playerReady && egitim?.videoUrl) {
            if (videoPlatform === 'YouTube') {
                initializeYouTubePlayer();
            } else if (videoPlatform === 'Vimeo') {
                initializeVimeoPlayer();
            }
        }
    }, [isMounted, playerReady, egitim?.videoUrl, videoPlatform]);

    // Set loading to false after a short delay if no API is needed
    useEffect(() => {
        if (!isMounted) return;
        
        if (egitim?.videoUrl && videoPlatform === 'Local') {
            // For local videos, set loading false immediately
            setIsLoading(false);
        } else if (egitim?.videoUrl && (videoPlatform === 'YouTube' || videoPlatform === 'Vimeo')) {
            // For YouTube/Vimeo, give APIs time to load, then set loading false if they haven't
            const timeout = setTimeout(() => {
                if (isLoading) {
            // console.log(`${videoPlatform} API taking too long, removing loading state`);
                    setIsLoading(false);
                }
            }, 5000); // 5 seconds timeout
            return () => clearTimeout(timeout);
        }
    }, [isMounted, egitim?.videoUrl, videoPlatform, isLoading]);

    const loadSavedProgress = async () => {
        if (egitim?.id && personelId) {
            try {
                // Get saved progress from backend
                const response = await videoEgitimService.getEgitimDetay(egitim.id, personelId);
                if (response.success && response.data) {
                    // Backend'den gelen veri yapısı: { egitim, izlemeKaydi, atama, ... }
                    const responseData = response.data;
                    const izlemeData = responseData.izlemeKaydi || 
                                      (responseData.VideoIzlemeler && responseData.VideoIzlemeler.length > 0 
                                        ? responseData.VideoIzlemeler[0] 
                                        : null);
                    
            // console.log('Loaded izleme data:', izlemeData);
                    
                    if (izlemeData) {
                        // Set saved progress
                        const savedPercentage = izlemeData.IzlemeYuzdesi || izlemeData.izlemeYuzdesi || 0;
                        const savedTime = izlemeData.ToplamIzlenenSure || izlemeData.toplamIzlenenSure || 0;
                        const dbCompletedStatus = izlemeData.TamamlandiMi || izlemeData.tamamlandiMi || false;

                        setWatchedPercentage(savedPercentage);
                        if (savedTime > 0) {
                            setCurrentTime(savedTime);
            // console.log(`Loading saved progress: ${savedPercentage}% at ${savedTime} seconds`);
                        }

                        // Store resume data for later use
                        setSavedProgress({
                            currentTime: savedTime,
                            percentage: savedPercentage,
                            completed: dbCompletedStatus
                        });

                        // Check if video was already completed in database
            // console.log(`Database completion status: ${dbCompletedStatus}`);
                        if (dbCompletedStatus) {
            // console.log('Video marked as completed in database - setting completed state');
                            setIsCompleted(true);
                            setHasShownCompletionToast(true); // Assume toast was shown before
                        } else {
            // console.log('Video not completed in database - resetting completion state');
                            setIsCompleted(false);
                            setHasShownCompletionToast(false);
                        }
                    } else {
            // console.log('No previous watch data found for this video');
                        // Reset to initial state for new video
                        setWatchedPercentage(0);
                        setCurrentTime(0);
                        setIsCompleted(false);
                        setSavedProgress(null);
                    }
                }
            } catch (error) {
            // console.error('Error loading saved progress:', error);
            }
        }
    };


    const loadYouTubeAPI = () => {
        if (typeof window === 'undefined') {
            // console.log('❌ Window object not available (SSR)');
            return;
        }
        
            // console.log('📡 Loading YouTube API...');
        if (window.YT && window.YT.Player) {
            // console.log('✅ YouTube API already loaded');
            setPlayerReady(true);
            return;
        }

        // Load YouTube IFrame Player API
        if (!document.getElementById('youtube-api')) {
            const script = document.createElement('script');
            script.id = 'youtube-api';
            script.src = 'https://www.youtube.com/iframe_api';
            document.body.appendChild(script);

            // Set up the callback for when API is ready with safety check
            const originalCallback = window.onYouTubeIframeAPIReady;
            window.onYouTubeIframeAPIReady = () => {
            // console.log('YouTube API loaded successfully');
                setPlayerReady(true);
                // Restore original callback if it existed
                if (originalCallback && typeof originalCallback === 'function') {
                    originalCallback();
                }
            };
        } else {
            // Script already exists, check if API is ready
            const checkAPI = setInterval(() => {
                if (window.YT && window.YT.Player) {
            // console.log('YouTube API is ready');
                    setPlayerReady(true);
                    clearInterval(checkAPI);
                }
            }, 100);
            
            // Clear interval after 5 seconds and set ready anyway
            setTimeout(() => {
                clearInterval(checkAPI);
                if (!playerReady) {
            // console.log('YouTube API timeout, setting ready anyway');
                    setPlayerReady(true);
                }
            }, 5000);
        }
    };

    const loadVimeoAPI = () => {
        if (typeof window === 'undefined') {
            // console.log('❌ Window object not available (SSR)');
            return;
        }
        
        if (window.Vimeo && window.Vimeo.Player) {
            setPlayerReady(true);
            return;
        }

        // Load Vimeo Player API
        if (!document.getElementById('vimeo-api')) {
            const script = document.createElement('script');
            script.id = 'vimeo-api';
            script.src = 'https://player.vimeo.com/api/player.js';
            script.onload = () => {
            // console.log('Vimeo API loaded successfully');
                setPlayerReady(true);
            };
            document.body.appendChild(script);
        } else {
            // Script already exists, check if API is ready
            const checkAPI = setInterval(() => {
                if (window.Vimeo && window.Vimeo.Player) {
            // console.log('Vimeo API is ready');
                    setPlayerReady(true);
                    clearInterval(checkAPI);
                }
            }, 100);
            
            // Clear interval after 5 seconds and set ready anyway
            setTimeout(() => {
                clearInterval(checkAPI);
                if (!playerReady) {
            // console.log('Vimeo API timeout, setting ready anyway');
                    setPlayerReady(true);
                }
            }, 5000);
        }
    };

    const handleVideoLoad = () => {
        setIsLoading(false);
        if (videoRef.current) {
            const actualDuration = videoRef.current.duration;
            setDuration(actualDuration);
            // console.log(`Local video loaded - Duration: ${actualDuration} seconds`);
        }
    };

    const handlePlay = () => {
        setIsPlaying(true);
        startProgressTracking();
    };

    const handlePause = () => {
        setIsPlaying(false);
        stopProgressTracking();
        saveCurrentProgress();
    };

    const handleTimeUpdate = () => {
            // console.log('handleTimeUpdate called!');

        if (videoRef.current && !videoRef.current.paused) {
            const current = videoRef.current.currentTime;
            const total = videoRef.current.duration;

            // console.log(`Video time update: current=${current}, total=${total}, paused=${videoRef.current.paused}`);

            if (total > 0) {
                const progressPercent = (current / total) * 100;
                setProgress(progressPercent);

            // console.log(`Progress calculation: ${progressPercent}% (current: ${current}s / total: ${total}s)`);

                // For local videos, currentTime is the position, watchedPercentage tracks total watched
                // Update watched percentage if user moved forward
                if (progressPercent > watchedPercentage) {
            // console.log(`Updating watched percentage from ${watchedPercentage}% to ${progressPercent}%`);
                    setWatchedPercentage(progressPercent);
                    setCurrentTime(current); // For local videos, this represents watched time
                }

                // Check completion using the current progressPercent (not state value)
                const requiredProgress = egitim?.izlenmeMinimum || 80;
            // console.log(`Completion check: ${Math.floor(progressPercent)}% >= ${requiredProgress}% ? ${progressPercent >= requiredProgress}, isCompleted: ${isCompleted}, hasShownCompletionToast: ${hasShownCompletionToast}`);

                if (progressPercent >= requiredProgress && !isCompleted && !hasShownCompletionToast) {
            // console.log(`🎉 Local video completion threshold reached: ${Math.floor(progressPercent)}% - TRIGGERING COMPLETION`);
                    handleVideoComplete();
                }

                // Auto-save progress periodically
                if (Math.floor(current) % 30 === 0 && Math.floor(current) > 0) {
            // console.log('Auto-saving progress...');
                    saveCurrentProgress();
                }

            // console.log(`Local video - Position: ${Math.floor(progressPercent)}%, Watched: ${Math.floor(watchedPercentage)}%, Time: ${Math.floor(current)}s/${Math.floor(total)}s`);
            }
        } else {
            // console.log('Video not available or paused');
        }
    };

    const startProgressTracking = () => {
        // For local videos, tracking is handled in handleTimeUpdate
        // For YouTube/Vimeo, tracking is handled in their respective initialization functions
        if (videoPlatform === 'Local') {
            // console.log('Local video progress tracking started via timeupdate events');
        } else {
            // console.log(`${videoPlatform} progress tracking handled by player API`);
        }
    };

    const stopProgressTracking = () => {
        if (progressIntervalRef.current) {
            clearInterval(progressIntervalRef.current);
            progressIntervalRef.current = null;
        }
        if (saveProgressIntervalRef.current) {
            clearInterval(saveProgressIntervalRef.current);
            saveProgressIntervalRef.current = null;
        }
    };

    const initializeLocalVideoTracking = () => {
            // console.log('Starting local video progress tracking');
        
        if (videoRef.current) {
            // For local videos, use the actual video duration once loaded
            videoRef.current.addEventListener('loadedmetadata', () => {
                const actualDuration = videoRef.current.duration;
                setDuration(actualDuration);
            // console.log(`Local video duration: ${actualDuration} seconds`);
                
                // Resume from saved position if available
                if (savedProgress && savedProgress.currentTime > 0 && savedProgress.currentTime < actualDuration) {
                    const resumeTime = Math.min(savedProgress.currentTime, actualDuration - 10);
            // console.log(`Resuming local video from ${resumeTime} seconds`);
                    videoRef.current.currentTime = resumeTime;
                    setCurrentTime(savedProgress.currentTime);
                    setWatchedPercentage(savedProgress.percentage || 0);
                }
            });
        } else {
            // Fallback to database duration if no video element
            const fallbackDuration = egitim?.sureDakika ? egitim.sureDakika * 60 : 300;
            setDuration(fallbackDuration);
            // console.log(`Using fallback duration: ${fallbackDuration} seconds`);
        }
    };

    const initializeYouTubePlayer = () => {
            // console.log('🎬 Initializing YouTube player...');
        if (typeof window === 'undefined') {
            // console.error('❌ Window object not available, cannot initialize YouTube player');
            return;
        }
        
        if (!window.YT || !window.YT.Player) {
            // console.error('❌ YouTube API not ready, using fallback iframe');
            createFallbackYouTubePlayer();
            return;
        }

        const videoId = getYouTubeVideoId(egitim.videoUrl);
            // console.log('🔍 Extracted YouTube video ID:', videoId);
        if (!videoId) {
            // console.error('❌ Could not extract YouTube video ID from URL:', egitim.videoUrl);
            setIsLoading(false);
            return;
        }

            // console.log('Creating YouTube player with videoId:', videoId);
        
        // Clear existing player container
        const container = document.getElementById('youtube-player');
        if (container) {
            container.innerHTML = '';
        } else {
            // console.error('❌ youtube-player element not found in DOM!');
            return;
        }

        try {
            const player = new window.YT.Player('youtube-player', {
                height: '450',
                width: '800',
                videoId: videoId,
                playerVars: {
                    autoplay: 0,
                    controls: 1,
                    modestbranding: 1,
                    rel: 0,
                    enablejsapi: 1
                },
                events: {
                    'onReady': (event) => {
            // console.log('YouTube player ready');
                        
                        // Ensure iframe is visible
                        const container = document.getElementById('youtube-player');
                        const iframe = container?.querySelector('iframe');
                        if (iframe) {
                            iframe.style.border = '0';
                            iframe.style.maxWidth = '100%';
                            iframe.style.height = 'auto';
                        }
                        
                        setYouTubePlayer(event.target);
                        initializeYouTubeTracking(event.target);
                        setIsLoading(false);
                    },
                    'onStateChange': (event) => {
                        const states = {
                            '-1': 'unstarted',
                            '0': 'ended',
                            '1': 'playing',
                            '2': 'paused',
                            '3': 'buffering',
                            '5': 'cued'
                        };
            // console.log('YouTube state change:', states[event.data]);
                        
                        if (event.data === 1) { // Playing
                            setIsPlaying(true);
                        } else if (event.data === 2) { // Paused
                            setIsPlaying(false);
                            saveCurrentProgress();
                        } else if (event.data === 0) { // Ended
                            setIsPlaying(false);
                            if (!isCompleted && !hasShownCompletionToast) {
            // console.log('YouTube video ended, triggering completion');
                                handleVideoComplete();
                            }
                        }
                    },
                    'onError': (event) => {
            // console.error('YouTube player error:', event.data);
                    }
                }
            });
        } catch (error) {
            // console.error('Error creating YouTube player:', error);
        }
    };

    const getYouTubeVideoId = (url) => {
        if (!url) return null;
        
        // Handle youtu.be format
        const youtuBeMatch = url.match(/youtu\.be\/([a-zA-Z0-9_-]{11})/);
        if (youtuBeMatch) return youtuBeMatch[1];
        
        // Handle youtube.com/watch format
        const watchMatch = url.match(/youtube\.com\/watch\?v=([a-zA-Z0-9_-]{11})/);
        if (watchMatch) return watchMatch[1];
        
        // Handle youtube.com/embed format
        const embedMatch = url.match(/youtube\.com\/embed\/([a-zA-Z0-9_-]{11})/);
        if (embedMatch) return embedMatch[1];
        
        return null;
    };

    // YouTube Player event handlers
    const initializeYouTubeTracking = (player) => {
            // console.log('Initializing YouTube tracking...');
        
        // Get actual duration from YouTube player
        const getDurationAndSetup = () => {
            const videoDuration = player.getDuration();
            if (videoDuration > 0) {
            // console.log(`YouTube video duration: ${videoDuration} seconds`);
                setDuration(videoDuration);
                
                // Resume from saved position if available
                if (savedProgress && savedProgress.currentTime > 0 && savedProgress.currentTime < videoDuration) {
                    const resumeTime = Math.min(savedProgress.currentTime, videoDuration - 10);
            // console.log(`Resuming YouTube video from ${resumeTime} seconds`);
                    player.seekTo(resumeTime, true);
                    setCurrentTime(savedProgress.currentTime);
                    setWatchedPercentage(savedProgress.percentage || 0);
                }
            } else {
                // Duration not ready yet, try again
                setTimeout(getDurationAndSetup, 1000);
            }
        };
        
        getDurationAndSetup();
        
        // Track progress
        let totalWatchedTime = savedProgress?.currentTime || 0;
        let lastUpdateTime = 0;
        let lastSaveTime = 0;
        
        const updateProgress = () => {
            try {
                if (player.getPlayerState() === 1) { // Playing
                    const currentVideoTime = player.getCurrentTime();
                    const videoDuration = player.getDuration();
                    
                    if (videoDuration > 0) {
                        // Only accumulate time if playing forward
                        if (currentVideoTime > lastUpdateTime && (currentVideoTime - lastUpdateTime) < 2) {
                            totalWatchedTime += (currentVideoTime - lastUpdateTime);
                        }
                        lastUpdateTime = currentVideoTime;
                        
                        // Calculate percentages
                        const calculatedWatchedPercentage = Math.min((totalWatchedTime / videoDuration) * 100, 100);
                        const currentPositionPercentage = (currentVideoTime / videoDuration) * 100;
                        
                        // Update state
                        setCurrentTime(totalWatchedTime);
                        setWatchedPercentage(calculatedWatchedPercentage);
                        setProgress(currentPositionPercentage);
                        
                        // Check completion
                        const requiredProgress = egitim?.izlenmeMinimum || 80;

                        // Debug completion conditions every 5%
                        if (Math.floor(calculatedWatchedPercentage) % 5 === 0 && Math.floor(calculatedWatchedPercentage) > 0) {
            // console.log(`YouTube completion check: ${Math.floor(calculatedWatchedPercentage)}% >= ${requiredProgress}% ? ${calculatedWatchedPercentage >= requiredProgress}, isCompleted: ${isCompleted}, hasShownCompletionToast: ${hasShownCompletionToast}`);
                        }

                        if (calculatedWatchedPercentage >= requiredProgress && !isCompleted && !hasShownCompletionToast) {
            // console.log(`YouTube completion threshold reached: ${Math.floor(calculatedWatchedPercentage)}% - TRIGGERING COMPLETION`);
                            handleVideoComplete();
                        }
                        
                        // Auto-save every 30 seconds
                        if (currentVideoTime - lastSaveTime > 30) {
                            saveCurrentProgress();
                            lastSaveTime = currentVideoTime;
                        }
                        
            // console.log(`YouTube - Position: ${Math.floor(currentPositionPercentage)}%, Watched: ${Math.floor(calculatedWatchedPercentage)}%, Total: ${Math.floor(totalWatchedTime)}s/${Math.floor(videoDuration)}s`);
                    }
                }
            } catch (error) {
            // console.error('Error in YouTube progress update:', error);
            }
        };
        
        // Update progress every 1 second
        progressIntervalRef.current = setInterval(updateProgress, 1000);
    };

    const initializeVimeoPlayer = () => {
        if (typeof window === 'undefined') {
            // console.error('❌ Window object not available, cannot initialize Vimeo player');
            return;
        }
        
        if (!window.Vimeo || !window.Vimeo.Player) {
            // console.error('Vimeo API not ready, using fallback iframe');
            createFallbackVimeoPlayer();
            return;
        }

        const videoId = getVimeoVideoId(egitim.videoUrl);
        if (!videoId) {
            // console.error('Could not extract Vimeo video ID');
            setIsLoading(false);
            return;
        }

            // console.log('Creating Vimeo player with videoId:', videoId);
        
        // Clear existing player container
        const container = document.getElementById('vimeo-player');
        if (container) {
            container.innerHTML = '';
        }

        try {
            const player = new window.Vimeo.Player('vimeo-player', {
                id: videoId,
                width: '100%',
                controls: true,
                autoplay: false,
                responsive: true
            });
            
            player.ready().then(() => {
            // console.log('Vimeo player ready');
                initializeVimeoTracking(player);
                setIsLoading(false);
            }).catch((error) => {
            // console.error('Vimeo player error:', error);
                setIsLoading(false);
            });
        } catch (error) {
            // console.error('Error creating Vimeo player:', error);
            setIsLoading(false);
        }
    };

    const getVimeoVideoId = (url) => {
        if (!url) return null;
        
        // Handle group videos: https://vimeo.com/groups/114/videos/1017406920
        const groupMatch = url.match(/vimeo\.com\/groups\/\d+\/videos\/(\d+)/);
        if (groupMatch) {
            return groupMatch[1];
        }
        
        // Handle regular videos: https://vimeo.com/1017406920
        const directMatch = url.match(/vimeo\.com\/(\d+)/);
        if (directMatch) {
            return directMatch[1];
        }
        
        return null;
    };

    // Vimeo Player event handlers
    const initializeVimeoTracking = (player) => {
            // console.log('Initializing Vimeo tracking...');

        let totalWatchedTime = savedProgress?.currentTime || 0;
        let lastUpdateTime = 0;
        let isPlayingState = false;
        let lastSaveTime = 0;
        
        // Get duration and setup resume
        player.getDuration().then(duration => {
            // console.log(`Vimeo video duration: ${duration} seconds`);
            setDuration(duration);
            
            // Resume from saved position if available
            if (savedProgress && savedProgress.currentTime > 0 && savedProgress.currentTime < duration) {
                const resumeTime = Math.min(savedProgress.currentTime, duration - 10);
            // console.log(`Resuming Vimeo video from ${resumeTime} seconds`);
                player.setCurrentTime(resumeTime);
                setCurrentTime(savedProgress.currentTime);
                setWatchedPercentage(savedProgress.percentage || 0);
            }
        }).catch(error => {
            // console.error('Error getting Vimeo duration:', error);
        });
        
        // Play event
        player.on('play', () => {
            // console.log('Vimeo play event');
            isPlayingState = true;
            setIsPlaying(true);
            player.getCurrentTime().then(currentTime => {
                lastUpdateTime = currentTime;
            });
        });
        
        // Pause event
        player.on('pause', () => {
            // console.log('Vimeo pause event');
            isPlayingState = false;
            setIsPlaying(false);
            saveCurrentProgress();
        });
        
        // Ended event
        player.on('ended', () => {
            // console.log('Vimeo ended event');
            isPlayingState = false;
            setIsPlaying(false);
            if (!isCompleted && !hasShownCompletionToast) {
            // console.log('Vimeo video ended, triggering completion');
                handleVideoComplete();
            }
        });
        
        // Time update
        player.on('timeupdate', (data) => {
            if (isPlayingState && data) {
                const currentVideoTime = data.seconds;
                const videoDuration = data.duration;
                
                if (videoDuration > 0) {
                    // Only accumulate time if playing forward
                    if (currentVideoTime > lastUpdateTime && (currentVideoTime - lastUpdateTime) < 2) {
                        totalWatchedTime += (currentVideoTime - lastUpdateTime);
                    }
                    lastUpdateTime = currentVideoTime;
                    
                    // Calculate percentages
                    const calculatedWatchedPercentage = Math.min((totalWatchedTime / videoDuration) * 100, 100);
                    const currentPositionPercentage = (currentVideoTime / videoDuration) * 100;
                    
                    // Update state with current values
                    setCurrentTime(totalWatchedTime);
                    setWatchedPercentage(calculatedWatchedPercentage);
                    setProgress(currentPositionPercentage);
                    setDuration(videoDuration);
                    
                    // Check completion using calculated value (not state)
                    const requiredProgress = egitim?.izlenmeMinimum || 80;

                    // Debug completion conditions every 5%
                    if (Math.floor(calculatedWatchedPercentage) % 5 === 0 && Math.floor(calculatedWatchedPercentage) > 0) {
            // console.log(`Vimeo completion check: ${Math.floor(calculatedWatchedPercentage)}% >= ${requiredProgress}% ? ${calculatedWatchedPercentage >= requiredProgress}, isCompleted: ${isCompleted}, hasShownCompletionToast: ${hasShownCompletionToast}`);
                    }

                    if (calculatedWatchedPercentage >= requiredProgress && !isCompleted && !hasShownCompletionToast) {
            // console.log(`🎉 Vimeo completion threshold reached: ${Math.floor(calculatedWatchedPercentage)}% - TRIGGERING COMPLETION`);
                        handleVideoComplete();
                    }
                    
                    // Auto-save every 30 seconds
                    if (currentVideoTime - lastSaveTime > 30) {
                        saveCurrentProgress();
                        lastSaveTime = currentVideoTime;
                    }
                    
            // console.log(`Vimeo - Position: ${Math.floor(currentPositionPercentage)}%, Watched: ${Math.floor(calculatedWatchedPercentage)}%, Total: ${Math.floor(totalWatchedTime)}s/${Math.floor(videoDuration)}s`);
                }
            }
        });
    };

    const saveCurrentProgress = async () => {
            // console.log(`Attempting to save progress - egitimId: ${egitim?.id}, personelId: ${personelId}`);
            // console.log('Egitim object:', egitim);
            // console.log('PersonelId value:', personelId);
        
        if (egitim?.id && personelId) {
            // console.log('Conditions met, preparing to save progress...');
            try {
                let progressData = {
                    videoEgitimId: egitim.id,
                    toplamIzlenenSure: Math.round(currentTime),
                    izlemeYuzdesi: Math.floor(watchedPercentage),
                    tamamlandiMi: isCompleted,
                    videoPlatform: videoPlatform,
                    videoToplamSure: Math.round(duration),
                    izlemeBaslangicSaniye: 0,
                    izlemeBitisSaniye: Math.round(currentTime),
                    izlemeBaslangic: new Date(),
                    izlemeBitis: isCompleted ? new Date() : null,
                    cihazTipi: 'Desktop' // Default device type
                };

                // Platform-specific progress data
                if (videoPlatform === 'YouTube' && youTubePlayer) {
                    // For YouTube, use total watched time (not current position)
                    progressData.toplamIzlenenSure = Math.round(currentTime);
                    progressData.videoToplamSure = Math.round(duration);
                    progressData.izlemeBitisSaniye = Math.round(currentTime);
                    progressData.izlemeYuzdesi = Math.floor(watchedPercentage);
                    
            // console.log(`Saving YouTube progress: ${progressData.izlemeYuzdesi}% (${Math.round(currentTime)}s watched of ${Math.round(duration)}s)`);
                } else if (videoPlatform === 'Vimeo') {
                    // Vimeo progress using current state values
                    progressData.toplamIzlenenSure = Math.round(currentTime);
                    progressData.videoToplamSure = Math.round(duration);
                    progressData.izlemeBitisSaniye = Math.round(currentTime);
                    progressData.izlemeYuzdesi = Math.floor(watchedPercentage);
                    
            // console.log(`Saving Vimeo progress: ${progressData.izlemeYuzdesi}% (${Math.round(currentTime)}s watched of ${Math.round(duration)}s)`);
                } else if (videoPlatform === 'Local' && videoRef.current) {
                    // Local video progress already handled above
            // console.log(`Saving Local video progress: ${progressData.izlemeYuzdesi}% (${Math.round(currentTime)}s watched of ${Math.round(duration)}s)`);
                }

                // Use videoEgitimService instead of direct fetch
                const result = await videoEgitimService.updateProgress(progressData);
                
                if (result.success) {
            // console.log('Progress saved successfully');
                } else {
            // console.error('Failed to save progress:', result.message);
                }
            } catch (error) {
            // console.error('Error saving progress:', error);
            }
        }
    };

    // Expose saveCurrentProgress function to parent components
    useImperativeHandle(ref, () => ({
        saveCurrentProgress: saveCurrentProgress
    }), [saveCurrentProgress]);

    // Effect for page unload handling - placed after saveCurrentProgress definition
    useEffect(() => {
        if (!isMounted) return;

        const handleBeforeUnload = (event) => {
            // Save progress before page unloads
            if (egitim?.id && personelId && watchedPercentage > 0 && !isCompleted) {
            // console.log('Page unloading, saving current progress...');
                // Use synchronous call for beforeunload to ensure it completes
                navigator.sendBeacon && navigator.sendBeacon('/api/VideoEgitim/update-progress', JSON.stringify({
                    videoEgitimId: egitim.id,
                    toplamIzlenenSure: Math.round(currentTime),
                    izlemeYuzdesi: Math.floor(watchedPercentage),
                    tamamlandiMi: isCompleted,
                    videoPlatform: videoPlatform,
                    videoToplamSure: Math.round(duration),
                    izlemeBaslangicSaniye: 0,
                    izlemeBitisSaniye: Math.round(currentTime),
                    cihazTipi: 'Desktop'
                }));
            }
        };

        const handleVisibilityChange = () => {
            // Save progress when page becomes hidden
            if (document.visibilityState === 'hidden' && egitim?.id && personelId && watchedPercentage > 0 && !isCompleted) {
            // console.log('Page hidden, saving current progress...');
                saveCurrentProgress();
            }
        };

        window.addEventListener('beforeunload', handleBeforeUnload);
        document.addEventListener('visibilitychange', handleVisibilityChange);

        return () => {
            window.removeEventListener('beforeunload', handleBeforeUnload);
            document.removeEventListener('visibilitychange', handleVisibilityChange);
        };
    }, [isMounted, egitim?.id, personelId, watchedPercentage, isCompleted, currentTime, videoPlatform, duration, saveCurrentProgress]);

    const handleVideoComplete = async () => {
            // console.log('handleVideoComplete called!');

        // Basic validation
        if (!egitim?.id || !personelId) {
            // console.log('Video completion blocked - missing egitim ID or personel ID:', { egitimId: egitim?.id, personelId });
            return;
        }

        // Check if already processing or completed to prevent multiple calls
        if (isProcessingCompletion || isCompleted || hasShownCompletionToast) {
            // console.log('Video completion blocked - already processed:', isProcessingCompletion, isCompleted, hasShownCompletionToast);
            return;
        }

        // Check session storage to prevent multiple completions across sessions
        const completionKey = `video_completion_${egitim?.id}_${personelId}`;
        const alreadyCompleted = sessionStorage.getItem(completionKey);
        if (alreadyCompleted) {
            // console.log('Video completion blocked - already completed in this session');
            return;
        }

            // console.log('Processing video completion...');
        setIsProcessingCompletion(true);
        setIsCompleted(true);
        setHasShownCompletionToast(true);

        // Mark as completed in session storage
        sessionStorage.setItem(completionKey, 'true');

        stopProgressTracking();

        try {
            // Get accurate current progress based on video platform
            let currentProgress = watchedPercentage;
            let totalWatchedTime = currentTime;

            if (videoPlatform === 'Local' && videoRef.current) {
                currentProgress = (videoRef.current.currentTime / videoRef.current.duration) * 100;
                totalWatchedTime = videoRef.current.currentTime;
            } else if (videoPlatform === 'Vimeo') {
                // For Vimeo, use current state values which should be updated by tracking
                currentProgress = watchedPercentage;
                totalWatchedTime = currentTime;
            } else if (videoPlatform === 'YouTube') {
                // For YouTube, use current state values
                currentProgress = watchedPercentage;
                totalWatchedTime = currentTime;
            }

            const progressData = {
                videoEgitimId: egitim.id,
                toplamIzlenenSure: Math.round(totalWatchedTime),
                izlemeYuzdesi: Math.floor(currentProgress),
                tamamlandiMi: true
            };

            // console.log('Sending completion data:', progressData);
            await videoEgitimService.izlemeKaydet(progressData);

            // Clear stored progress since video is completed
            videoEgitimService.clearStoredProgress(egitim.id);

            // Show success toast
            toast.current?.show({
                severity: 'success',
                summary: 'Tebrikler!',
                detail: 'Eğitimi başarıyla tamamladınız.',
                life: 5000
            });

            if (onComplete) {
                onComplete(egitim);
            }
        } catch (error) {
            // console.error('Error completing video:', error);

            // Reset completion state on error to allow retry
            setIsCompleted(false);

            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Tamamlanma durumu kaydedilemedi.',
                life: 3000
            });
        } finally {
            setIsProcessingCompletion(false);
        }
    };

    const togglePlay = () => {
        if (videoRef.current) {
            if (isPlaying) {
                videoRef.current.pause();
            } else {
                videoRef.current.play();
            }
        }
    };

    const handleSeek = (e) => {
        if (videoRef.current) {
            const rect = e.currentTarget.getBoundingClientRect();
            const clickX = e.clientX - rect.left;
            const newTime = (clickX / rect.width) * duration;
            videoRef.current.currentTime = newTime;
            setCurrentTime(newTime);
        }
    };

    const handleVolumeChange = (newVolume) => {
        if (videoRef.current) {
            videoRef.current.volume = newVolume;
            setVolume(newVolume);
        }
    };

    const toggleFullscreen = () => {
        if (!isFullscreen) {
            if (videoRef.current?.requestFullscreen) {
                videoRef.current.requestFullscreen();
            }
        } else {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            }
        }
        setIsFullscreen(!isFullscreen);
    };

    const formatTime = (time) => {
        const hours = Math.floor(time / 3600);
        const minutes = Math.floor((time % 3600) / 60);
        const seconds = Math.floor(time % 60);

        if (hours > 0) {
            return `${hours}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        }
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    };

    const getEmbedUrl = (url) => {
        return videoEgitimService.getVideoEmbedUrl(url);
    };

    // Fallback players for when APIs don't load
    const createFallbackYouTubePlayer = () => {
        const container = document.getElementById('youtube-player');
        if (container) {
            const embedUrl = getEmbedUrl(egitim.videoUrl);
            container.innerHTML = `
                <iframe 
                    src="${embedUrl}" 
                    width="100%" 
                    height="400" 
                    frameborder="0" 
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                    allowfullscreen
                    style="position: absolute; top: 0; left: 0; width: 100%; height: 100%;"
                ></iframe>
            `;
            setIsLoading(false);
            // console.log('YouTube fallback iframe created');
        }
    };

    const createFallbackVimeoPlayer = () => {
        const container = document.getElementById('vimeo-player');
        if (container) {
            const embedUrl = getEmbedUrl(egitim.videoUrl);
            container.innerHTML = `
                <iframe 
                    src="${embedUrl}" 
                    width="100%" 
                    height="400" 
                    frameborder="0" 
                    allow="autoplay; fullscreen; picture-in-picture" 
                    allowfullscreen
                    style="position: absolute; top: 0; left: 0; width: 100%; height: 100%;"
                ></iframe>
            `;
            setIsLoading(false);
            // console.log('Vimeo fallback iframe created');
        }
    };


    if (!egitim) {
        return <div>Video eğitim bulunamadı.</div>;
    }

    // Don't render anything during SSR to prevent hydration mismatch
    if (!isMounted) {
        return (
            <div className="video-player-container">
                <Card className="video-player-card">
                    <div className="video-loading">
                        <i className="pi pi-spin pi-spinner"></i>
                        <span>Video player yükleniyor...</span>
                    </div>
                </Card>
            </div>
        );
    }

    const getVideoStatus = () => {
        if (watchedPercentage === 0) {
            return "Yeni";
        } else if (watchedPercentage >= (egitim?.izlenmeMinimum || 80)) {
            return "Tamamlandı";
        } else {
            return "Devam Ediyor";
        }
    };

    const getVideoStatusSeverity = () => {
        if (watchedPercentage === 0) {
            return "info";
        } else if (watchedPercentage >= (egitim?.izlenmeMinimum || 80)) {
            return "success";
        } else {
            return "warning";
        }
    };

    const isYouTubeVideo = egitim.videoUrl?.includes('youtube.com') || egitim.videoUrl?.includes('youtu.be');
    const isVimeoVideo = egitim.videoUrl?.includes('vimeo.com');

    return (
        <div className="video-player-container">
            <Toast ref={toast} />
            
            <Card className="video-player-card">
                <div className="video-header">
                    <h2 className="video-title">{egitim.baslik}</h2>
                    <div className="video-meta">
                        <Badge 
                            value={egitim.seviye} 
                            severity={videoEgitimService.getLevelBadgeClass(egitim.seviye)} 
                        />
                        {egitim.zorunluMu && (
                            <Badge value="Zorunlu" severity="danger" />
                        )}
                        <span className="duration">
                            <i className="pi pi-clock"></i>
                            {videoEgitimService.formatDuration(egitim.sureDakika)}
                        </span>
                        <span className="instructor">
                            <i className="pi pi-user"></i>
                            {egitim.egitmen}
                        </span>
                    </div>
                </div>

                <div className="video-wrapper">
                    {isLoading && (
                        <div className="video-loading">
                            <i className="pi pi-spin pi-spinner"></i>
                            <span>Video yükleniyor...</span>
                        </div>
                    )}
                    
                    {isYouTubeVideo ? (
                        <div 
                            id="youtube-player" 
                            style={{ 
                                width: '100%',
                                height: '450px',
                                position: 'relative',
                                background: '#000',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center'
                            }}
                        >
                            {/* YouTube player will be inserted here by API */}
                        </div>
                    ) : isVimeoVideo ? (
                        <div 
                            id="vimeo-player" 
                            style={{ 
                                position: 'relative',
                                paddingBottom: '56.25%', // 16:9 aspect ratio
                                height: 0,
                                overflow: 'hidden'
                            }}
                        >
                            {/* Vimeo player will be inserted here by API */}
                        </div>
                    ) : (
                        <div className="custom-video-player">
                            <video
                                ref={videoRef}
                                src={egitim.videoUrl}
                                onLoadedData={() => {
                                    handleVideoLoad();
                                    initializeLocalVideoTracking();
                                }}
                                onPlay={handlePlay}
                                onPause={handlePause}
                                onTimeUpdate={handleTimeUpdate}
                                onEnded={handleVideoComplete}
                                className="video-element"
                                preload="metadata"
                            />
                            
                            <div className="video-controls">
                                <Button
                                    icon={isPlaying ? "pi pi-pause" : "pi pi-play"}
                                    onClick={togglePlay}
                                    className="p-button-rounded control-button"
                                />
                                
                                <div className="progress-container" onClick={handleSeek}>
                                    <div className="progress-bar">
                                        <div 
                                            className="progress-filled"
                                            style={{ width: `${progress}%` }}
                                        />
                                    </div>
                                </div>
                                
                                <span className="time-display">
                                    {formatTime(currentTime)} / {formatTime(duration)}
                                </span>
                                
                                <div className="volume-control">
                                    <i className="pi pi-volume-up"></i>
                                    <input
                                        type="range"
                                        min="0"
                                        max="1"
                                        step="0.1"
                                        value={volume}
                                        onChange={(e) => handleVolumeChange(parseFloat(e.target.value))}
                                        className="volume-slider"
                                    />
                                </div>
                                
                                <Button
                                    icon="pi pi-window-maximize"
                                    onClick={toggleFullscreen}
                                    className="p-button-rounded control-button"
                                />
                            </div>
                        </div>
                    )}
                </div>

                {/* Progress Information */}
                <div className="progress-info">
                    <div className="progress-stats">
                        <div className="progress-item">
                            <span>Toplam İzlenme Oranı</span>
                            <ProgressBar 
                                value={watchedPercentage} 
                                style={{ height: '10px' }}
                                className="mb-2"
                            />
                            <div className="flex justify-content-between">
                                <span className="font-bold">{Math.floor(watchedPercentage)}%</span>
                                <span className="text-sm text-500">
                                    {formatTime(currentTime)} / {formatTime(duration)}
                                </span>
                            </div>
                        </div>
                        
                        <div className="progress-item">
                            <div className="flex justify-content-between align-items-center">
                                <span>Gerekli Oran: <strong>{egitim.izlenmeMinimum}%</strong></span>
                                <Badge 
                                    value={getVideoStatus()}
                                    severity={getVideoStatusSeverity()}
                                />
                            </div>
                        </div>
                        
                        <div className="progress-item">
                            <div className="flex justify-content-between align-items-center">
                                <span>Video Platform: <strong>{videoPlatform}</strong></span>
                            </div>
                        </div>
                        
                    </div>
                </div>

                {egitim.aciklama && (
                    <>
                        <Divider />
                        <div className="video-description">
                            <h4>Açıklama</h4>
                            <p>{egitim.aciklama}</p>
                        </div>
                    </>
                )}

            </Card>
        </div>
    );
});

VideoPlayer.displayName = 'VideoPlayer';

export default VideoPlayer;